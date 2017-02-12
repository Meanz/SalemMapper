using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SalemMapper
{

    class TileCacheLRU
    {
        private Dictionary<string, Bitmap> m_dict;
        private LinkedList<string> m_lru_cache;
        private int m_capacity;

        public delegate void RemovedItemHandler(object sender, string key, Bitmap value);
        public event RemovedItemHandler Removed;

        public TileCacheLRU(int capacity)
        {
            m_capacity = capacity;
            m_dict = new Dictionary<string, Bitmap>();
            m_lru_cache = new LinkedList<string>();
        }

        public Bitmap Get(string key)
        {
            Bitmap bitmap = null;
            m_dict.TryGetValue(key, out bitmap);

            if(bitmap != null)
            {
                //Make this entry the LRU
                m_lru_cache.Remove(key);
                m_lru_cache.AddLast(key);
            }

            return bitmap;
        }

        public void Insert(string key, Bitmap bitmap)
        {

            if(m_lru_cache.Count > m_capacity)
            {
                removeLRU();
            }

            //add to dictionary
            m_dict[key] = bitmap;

            //add to lru cache
            m_lru_cache.AddLast(key);

        }

        private void removeLRU()
        {
            string lru = m_lru_cache.First();
            m_lru_cache.RemoveFirst();
            Bitmap bm = m_dict[lru];
            m_dict.Remove(lru);
            if(Removed != null)
            {
                Removed(this, lru, bm);
            }
        }

    }

    static class TileCache
    {

        //private static Dictionary<string, Bitmap> file_to_image = new Dictionary<string, Bitmap>();
        private static Dictionary<Bitmap, BitmapData> bitmap_to_bitmap_data = new Dictionary<Bitmap, BitmapData>();
        private static Mutex tile_cache_mutex = new Mutex();
        private static TileCacheLRU lru_cache = new TileCacheLRU(5000); //500 * (
        private static List<Bitmap> m_active_bitmaps = new List<Bitmap>();

        public static void Initialize()
        {
            lru_cache.Removed += Lru_cache_Removed;
        }

        private static void Lru_cache_Removed(object sender, string key, Bitmap value)
        {
            //The mutex will already be locked when this function is called
            BitmapData bmd = null;
            bitmap_to_bitmap_data.TryGetValue(value, out bmd);
            if(bmd != null)
            {
                value.UnlockBits(bmd);
                bitmap_to_bitmap_data.Remove(value);
            }
            m_active_bitmaps.Remove(value);
            value.Dispose(); //dispose of our item
        }

        public static BitmapData FetchImageData(string path)
        {
            tile_cache_mutex.WaitOne();
            Bitmap img = null;
            {
                img = lru_cache.Get(path);
            }

            if (img == null)
            {
                img = (Bitmap)Image.FromFile(path);
                if (img == null)
                {
                    tile_cache_mutex.ReleaseMutex();
                    return null;
                }
                {
                    m_active_bitmaps.Add(img);
                    lru_cache.Insert(path, img);
                }

            }

            //Check if this image was disposed somehow.
            BitmapData bd = null;
            bitmap_to_bitmap_data.TryGetValue(img, out bd);
            if (bd == null)
            {
                bd = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
                bitmap_to_bitmap_data[img] = bd;
            }

            tile_cache_mutex.ReleaseMutex();
            return bd;
        }

        /*public static void Dispose(string path)
        {
            tile_cache_mutex.WaitOne();
            Bitmap img = null;
            file_to_image.TryGetValue(path, out img);
            if (img != null)
            {
                BitmapData bd = null;
                bitmap_to_bitmap_data.TryGetValue(img, out bd);
                if (bd != null)
                {
                    img.UnlockBits(bd);
                    bitmap_to_bitmap_data.Remove(img);
                }
                file_to_image.Remove(path);
                img.Dispose();
            }
            tile_cache_mutex.ReleaseMutex();
        }*/

    }
}
