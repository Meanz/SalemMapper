using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SalemMapper
{

    public partial class Form1 : Form
    {
        private List<Session> m_sessions = null;
        private Bitmap m_offscreen_image = null;
        private Graphics m_offscreen_graphics = null;

        private List<MegaSession> m_mega_sessions;
        private MegaSession m_active_mega_session;
        private bool m_running;
        //The list of things to process later
        private LinkedList<Session> m_unprocessed_sessions = null;
        private List<Session> process_later = new List<Session>();
        private Mutex m_process_later_mutex = new Mutex();
        private Mutex m_sessions_mutex = new Mutex();
        private Mutex m_mega_session_mutex = new Mutex();
        private Mutex m_update_text_mutex = new Mutex();

        private List<Thread> m_thread_pool;

        //
        public void RenderPanel(Graphics g)
        {
            if(m_sessions == null)
            {
                return;
            }

            //Create our offscreen render target
            if(m_offscreen_image == null ||
                m_offscreen_image.Size != pnlMap.Size)
            {
                if(m_offscreen_graphics != null)
                {
                    m_offscreen_graphics.Dispose();
                }
                m_offscreen_image = new Bitmap(pnlMap.Size.Width, pnlMap.Size.Height);
                m_offscreen_graphics = Graphics.FromImage(m_offscreen_image);
            }

            //Fill entire screen with blackness
            g.FillRectangle(Brushes.Black, 0, 0, Size.Width, Size.Height);

            //Fill old image with blackness
            m_offscreen_graphics.FillRectangle(Brushes.Black, 0, 0, Size.Width, Size.Height);
            
            //Render our active mega session
            if(m_active_mega_session != null)
            {
                m_active_mega_session.Render(m_map_offset, m_offscreen_graphics);
            }

            //Blit render target to main frame
            g.DrawImage(m_offscreen_image, 0, 0);
        }

        //
        private bool m_dragging;
        private int m_mouse_x;
        private int m_mouse_y;
        private Vector2i m_map_offset;

        /// <summary>
        /// 
        /// </summary>
        private static volatile int m_thread_val = 0;
        public void ThreadProc()
        {
            while(m_running && !IsDisposed)
            {
                //Grab a session
                m_sessions_mutex.WaitOne();
                Session session = null;
                if (m_unprocessed_sessions.Count > 0)
                {
                    session = m_unprocessed_sessions.First();
                    m_unprocessed_sessions.RemoveFirst();
                    m_sessions_mutex.ReleaseMutex();
                }
                else
                {
                    m_sessions_mutex.ReleaseMutex();

                    //Activate step two!
                    //Wait for all threads to complete!
                    MegaSession _ms = m_active_mega_session;
                    //Memory barrier
                    Interlocked.Increment(ref m_thread_val);
                    while (m_thread_val != m_thread_pool.Count){}

                    m_sessions_mutex.WaitOne();
                    m_mega_session_mutex.WaitOne();
                    m_process_later_mutex.WaitOne();
                    if (process_later.Count > 0)
                    {
                        if (_ms == m_active_mega_session)
                        {
                            //Move all process later into process now!
                            foreach (Session s in process_later)
                            {
                                m_unprocessed_sessions.AddLast(s);
                            }

                            //Fetch first as our session
                            Session os = m_unprocessed_sessions.First();
                            m_unprocessed_sessions.RemoveFirst();

                            //Clear process later
                            process_later.Clear();

                            m_active_mega_session = new MegaSession();
                            m_active_mega_session.SetOrigoSession(os);
                            m_mega_sessions.Add(m_active_mega_session);

                            m_thread_val = 0;
                        }
                    }
                    m_process_later_mutex.ReleaseMutex();
                    m_mega_session_mutex.ReleaseMutex();
                    m_sessions_mutex.ReleaseMutex();

                }

                if (session == null) continue;

                //Now process it
                if (!m_active_mega_session.Analyze(session))
                {
                    m_process_later_mutex.WaitOne();
                    process_later.Add(session);
                    m_process_later_mutex.ReleaseMutex();
                }

                m_update_text_mutex.WaitOne();
                if(!status.IsDisposed && IsHandleCreated)
                {
                    int pgbValue = (100 - (100 * (m_unprocessed_sessions.Count + process_later.Count) / m_sessions.Count));

                    status.BeginInvoke((MethodInvoker)(() => lblUnproc.Text = "Unprocessed Sessions: " + m_unprocessed_sessions.Count));
                    status.BeginInvoke((MethodInvoker)(() => lblBackproc.Text = "Process Later: " + process_later.Count));
                    status.BeginInvoke((MethodInvoker)(() => pgbTotal.Value = pgbValue));
                   // this.BeginInvoke((MethodInvoker)(() => Refresh()));
                }
                m_update_text_mutex.ReleaseMutex();

                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Analyze()
        {
            m_active_mega_session = new MegaSession();
            m_active_mega_session.SetOrigoSession(m_sessions.First());
            m_mega_sessions.Add(m_active_mega_session);

            //Create one thread per core! Because we are "hard" core, HUE HUE!
            //Perhaps add System.Management to get core count? 
            //http://stackoverflow.com/questions/1542213/how-to-find-the-number-of-cpu-cores-via-net-c
            //For now, just static
            int core_count = 8;

            //Set running to true!
            m_running = true;

            //Create jobs!
            m_unprocessed_sessions = new LinkedList<Session>();
            foreach(Session session in m_sessions)
            {
                if (session == m_sessions.First()) continue;
                m_unprocessed_sessions.AddLast(session);
            }

            //Spawn our threads
            for(int j=0; j < core_count - 1; j++)
            {
                Thread t = new Thread(new ThreadStart(ThreadProc));
                t.Name = "salem_mapper_worker_" + (j + 1);
                t.Start();
                
                m_thread_pool.Add(t);
            }

        }

        public Form1()
        {
            TileCache.Initialize();

            this.DoubleBuffered = true;
            InitializeComponent();

            m_mega_sessions = new List<MegaSession>();
            m_active_mega_session = null;
            m_thread_pool = new List<Thread>();

            m_dragging = false;
            m_mouse_x = 0;
            m_mouse_y = 0;
            m_map_offset = new Vector2i();

            Console.WriteLine("Looking for sessions.");
            m_sessions = SessionCrawler.CrawlSessions("C:\\Users\\DesktopMeanz\\Salem\\map\\game.salemthegame.com");
            Console.WriteLine("Found " + m_sessions.Count + " sessions.");

            Analyze();

            //Grab a session and render it
            pnlMap.Paint += PnlMap_Paint;
            pnlMap.MouseDown += PnlMap_MouseDown;
            pnlMap.MouseUp += PnlMap_MouseUp;
            pnlMap.MouseMove += PnlMap_MouseMove;
        }

        private void PnlMap_MouseMove(object sender, MouseEventArgs e)
        {
            if(m_dragging)
            {

                int dx = e.X - m_mouse_x;
                int dy = e.Y - m_mouse_y;

                m_map_offset += new Vector2i(dx, dy);

                //Repaint form
                Refresh();
            }

            m_mouse_x = e.X;
            m_mouse_y = e.Y;
        }

        private void PnlMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                m_dragging = false;
            }

            m_mouse_x = e.X;
            m_mouse_y = e.Y;
        }

        private void PnlMap_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button ==  MouseButtons.Left)
            {
                m_dragging = true;
            }

            m_mouse_x = e.X;
            m_mouse_y = e.Y;
        }

        private void PnlMap_Paint(object sender, PaintEventArgs e)
        {
            RenderPanel(e.Graphics);
        }
    }
}
