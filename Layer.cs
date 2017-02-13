using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Drawing.Imaging;

namespace SalemMapper
{

    [Serializable]
    class SessionHistoryEntry
    {
        /// <summary>
        /// The session name
        /// </summary>
        public string Session;

        /// <summary>
        /// The X coordinate in the session
        /// </summary>
        public int SessionX;

        /// <summary>
        /// The Y coordinate in the session
        /// </summary>
        public int SessionY;
    }

    [Serializable]
    class LayerTile
    {

        /// <summary>
        /// The different revisions of this mega session tile
        /// </summary>
        public List<SessionHistoryEntry> SessionHistory = new List<SessionHistoryEntry>();

        /// <summary>
        /// The name of the current session for this mega session tile
        /// </summary>
        public string CurrentSession;

        /// <summary>
        /// Coordinates in the session
        /// </summary>
        public int LocalX, LocalY;

        /// <summary>
        /// Coordinates in our map system
        /// </summary>
        public int GlobalX, GlobalY;

        /// <summary>
        /// The tile.png file path
        /// </summary>
        public string TileFilePath;

        /// <summary>
        /// The reference to this tile's session
        /// </summary>
        [NonSerialized]
        public Session SessionRef;

        /// <summary>
        /// The reference to this tile
        /// </summary>
        [NonSerialized]
        public Tile SessionTileRef;

        [NonSerialized]
        private Image m_image;
        public Image Image {
            get
            {
                if(m_image == null)
                {
                    m_image = Image.FromFile(TileFilePath);
                }
                return m_image;
            }
        }
    }

    [Serializable]
    class Layer
    {
        /// <summary>
        /// 
        /// </summary>
        private List<LayerTile> m_tiles;

        /// <summary>
        /// 
        /// </summary>
        [NonSerialized]
        private Mutex m_tile_mutex;

        /// <summary>
        /// 
        /// </summary>
        [NonSerialized]
        private RenderTarget m_render_target;

        /// <summary>
        /// The bounds of this mega session
        /// </summary>
        [NonSerialized]
        private Rectangle m_bounds;

        /// <summary>
        /// Constructs a new mega session
        /// </summary>
        public Layer()
        {
            m_tiles = new List<LayerTile>();
            m_tile_mutex = new Mutex();     
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

                LayerTile lt = new LayerTile();
                lt.CurrentSession = session.Name;
                lt.GlobalX = tile.GlobalX;
                lt.GlobalY = tile.GlobalY;
                lt.LocalX = tile.LocalX;
                lt.LocalY = tile.LocalY;
                lt.TileFilePath = tile.TileFilePath;

                SessionHistoryEntry history_entry = new SessionHistoryEntry();
                history_entry.SessionX = 0;
                history_entry.SessionY = 0;
                history_entry.Session = session.Name;
                lt.SessionHistory.Add(history_entry);

                //Non serializeable
                lt.SessionRef = session;
                lt.SessionTileRef = tile;

                m_tiles.Add(lt);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="threshold"></param>
        /// <returns></returns>
        private bool Compare(Tile a, Tile b, double threshold)
        {
            BitmapData bda = a.GetBitmapData();
            BitmapData bdb = b.GetBitmapData();
            int equality = 0;
            unsafe
            {
                byte* ptr1 = (byte*)bda.Scan0.ToPointer();
                byte* ptr2 = (byte*)bdb.Scan0.ToPointer();
                int width = bda.Width * 4;
                for (int y = 0; y < bda.Height; y++)
                {
                    for (int x = 0; x < bda.Width; x++)
                    {
                        int argb_a = *((int*)ptr1);
                        int argb_b = *((int*)ptr2);
                        if (argb_a == argb_b)
                        {
                            equality++;
                        }
                        ptr1 += 4;
                        ptr2 += 4;
                    }
                    ptr1 += bda.Stride - width;
                    ptr2 += bdb.Stride - width;
                }
            }
            double equality_perc = (equality / (double)(bda.Width * bda.Height)) * 100;
            if (equality_perc > threshold)
            {
                return true;
            }
            return false;

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
            int match_tile_x = 0;
            int match_tile_y = 0;

            //Make a local copy of the tile list
            m_tile_mutex.WaitOne();
            LayerTile[] our_tiles = new LayerTile[m_tiles.Count];
            int idx = 0;
            foreach(LayerTile mst in m_tiles)
            {
                our_tiles[idx++] = mst;
            }
            m_tile_mutex.ReleaseMutex();

            //Match tiles!
            foreach(Tile in_session_tile in in_session.Tiles)
            {
                foreach(LayerTile layer_tile in our_tiles)
                {
                    if(Compare(in_session_tile, layer_tile.SessionTileRef, 90.0))
                    {
                        //We have a match!
                        in_session_tile.GlobalX = layer_tile.GlobalX;
                        in_session_tile.GlobalY = layer_tile.GlobalY;

                        //if this tile is -1, -2
                        //Then our session global x would be (other_x + (1)) or (other_x - (-1))
                        session_x = layer_tile.GlobalX - in_session_tile.LocalX;
                        session_y = layer_tile.GlobalY - in_session_tile.LocalY;

                        match_tile_x = in_session_tile.LocalX;
                        match_tile_y = in_session_tile.LocalY;

                        was_match = true;
                        break;
                    }
                }
                if(was_match)
                {
                    break;
                }
            }

            if(was_match)
            {
                //Also attempt to look at neighbours
                foreach(Tile in_tile in in_session.Tiles)
                {
                    in_tile.GlobalX = session_x + in_tile.LocalX;
                    in_tile.GlobalY = session_y + in_tile.LocalY;

                    //Find that tiles in our tiles repo
                    foreach (LayerTile layer_tile in our_tiles)
                    {
                        if(in_tile.GlobalX == layer_tile.GlobalX &&
                            in_tile.GlobalY == layer_tile.GlobalY)
                        {
                            //Attempt to match this tile to
                            if(!Compare(in_tile, layer_tile.SessionTileRef, 90.0))
                            {
                                return false;
                            }
                        }
                    }
                }

                m_tile_mutex.WaitOne();
                foreach (Tile tile in in_session.Tiles)
                {
                    tile.GlobalX = session_x + tile.LocalX;
                    tile.GlobalY = session_y + tile.LocalY;

                    //T_T
                    List<LayerTile> to_remove = new List<LayerTile>();
                    foreach(LayerTile layer_tile in m_tiles)
                    {
                        if(layer_tile.GlobalX == tile.GlobalX &&
                            layer_tile.GlobalY == tile.GlobalY)
                        {
                            to_remove.Add(layer_tile);
                        }
                    }
                    foreach(LayerTile remove_tile in to_remove)
                    {
                        m_tiles.Remove(remove_tile);
                    }

                    LayerTile lt = new LayerTile();
                    lt.CurrentSession = in_session.Name;
                    lt.GlobalX = tile.GlobalX;
                    lt.GlobalY = tile.GlobalY;
                    lt.LocalX = tile.LocalX;
                    lt.LocalY = tile.LocalY;
                    lt.TileFilePath = tile.TileFilePath;

                    SessionHistoryEntry history_entry = new SessionHistoryEntry();
                    history_entry.SessionX = 0;
                    history_entry.SessionY = 0;
                    lt.SessionHistory.Add(history_entry);

                    //Non serializeable
                    lt.SessionRef = in_session;
                    lt.SessionTileRef = tile;

                    m_tiles.Add(lt);

                    //Also save the tile!
                    Database.SaveTile(0, lt);
                }

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

            m_tile_mutex.WaitOne();

            foreach (LayerTile t in m_tiles)
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
