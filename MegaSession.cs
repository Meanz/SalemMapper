using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SalemMapper
{
    class MegaSession
    {
        /// <summary>
        /// 
        /// </summary>
        private List<Tile> m_tiles;

        /// <summary>
        /// 
        /// </summary>
        private Mutex m_tile_mutex;

        /// <summary>
        /// 
        /// </summary>
        private volatile int m_tile_lock;

        /// <summary>
        /// 
        /// </summary>
        private RenderTarget m_render_target;

        /// <summary>
        /// The bounds of this mega session
        /// </summary>
        private Rectangle m_bounds;

        /// <summary>
        /// Whether or not this session is dirty
        /// </summary>
        private bool m_is_dirty;

        private HighResolutionTimer m_timer = new HighResolutionTimer();

        /// <summary>
        /// Constructs a new mega session
        /// </summary>
        public MegaSession()
        {
            m_tiles = new List<Tile>();
            m_tile_mutex = new Mutex();
            m_tile_lock = 0;
            m_render_target = new RenderTarget();
        }

        /// <summary>
        /// Used to add the initial session to this Mega Session
        /// </summary>
        public void SetOrigoSession(Session session)
        {
            foreach(Tile tile in session.Tiles)
            {
                tile.GlobalX = tile.LocalX;
                tile.GlobalY = tile.LocalY;
                m_tiles.Add(tile);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="session"></param>
        public bool Analyze(Session in_session)
        {
            bool was_match = false;
            int session_x = 0;
            int session_y = 0;

            //Make a local copy of the tile list
            List<Tile> our_tiles = new List<Tile>();

            m_tile_mutex.WaitOne();
            our_tiles.AddRange(m_tiles);
            m_tile_mutex.ReleaseMutex();

            //Match tiles!
            m_timer.Start();
            foreach(Tile in_session_tile in in_session.Tiles)
            {
                foreach(Tile session_tile in our_tiles)
                {
                    if(SessionAnalyzer.Matches(in_session_tile, session_tile))
                    {
                        //We have a match!
                        in_session_tile.GlobalX = session_tile.GlobalX;
                        in_session_tile.GlobalY = session_tile.GlobalY;

                        //if this tile is -1, -2
                        //Then our session global x would be (other_x + (1)) or (other_x - (-1))
                        session_x = session_tile.GlobalX - in_session_tile.LocalX;
                        session_y = session_tile.GlobalY - in_session_tile.LocalY;

                        was_match = true;
                        break;
                    }
                }
                if(was_match)
                {
                    break;
                }
            }

            if (!was_match)
            {
                //Free our resources
                foreach(Tile tile in in_session.Tiles)
                {
                    tile.DisposeCopy();
                }
            }

            m_timer.Stop();
           // Console.WriteLine("Comparison: " + m_timer.ElapsedMilliseconds + "ms");
            m_timer.Reset();

            if(was_match)
            {
                m_tile_mutex.WaitOne();
                Interlocked.Add(ref m_tile_lock, 1);
                foreach (Tile tile in in_session.Tiles)
                {
                    tile.GlobalX = session_x + tile.LocalX;
                    tile.GlobalY = session_y + tile.LocalY;

                    //T_T
                    List<Tile> to_remove = new List<Tile>();
                    foreach(Tile mega_tile in m_tiles)
                    {
                        if(mega_tile.GlobalX == tile.GlobalX &&
                            mega_tile.GlobalY == tile.GlobalY)
                        {
                            to_remove.Add(mega_tile);
                        }
                    }
                    foreach(Tile remove_tile in to_remove)
                    {
                        remove_tile.DisposeCopy();
                        m_tiles.Remove(remove_tile);
                    }
                    m_tiles.Add(tile);
                }
                Interlocked.Decrement(ref m_tile_lock);
                m_tile_mutex.ReleaseMutex();
            }

            return was_match;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="g"></param>
        public void Render(Vector2i pos, Graphics graphics)
        {
            /*
            foreach(Session session in m_sessions)
            {
                //Calculate new offset!
                int offset_x = pos.X;
                int offset_y = pos.Y;
                session.Render(pos, g);
            }
            */
            if(m_tile_lock > 0)
            {
                return;
            }

            m_tile_mutex.WaitOne();

            foreach (Tile t in m_tiles)
            {
                Image image = t.Image;
                if (image != null)
                {
                    graphics.DrawImage(image,
                        pos.X + (t.GlobalX * image.Width),
                        pos.Y + (t.GlobalY * image.Height));
                }
            }

            m_tile_mutex.ReleaseMutex();
        }

    }
}
