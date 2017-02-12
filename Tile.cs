using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;

namespace SalemMapper
{
    static class TileCache
    {

        private static Dictionary<string, Bitmap> file_to_image = new Dictionary<string, Bitmap>();
        private static Dictionary<Bitmap, BitmapData> bitmap_to_bitmap_data = new Dictionary<Bitmap, BitmapData>();
        private static Mutex tile_cache_mutex = new Mutex();

        public static BitmapData FetchImageData(string path)
        {
            tile_cache_mutex.WaitOne();
            Bitmap img = null;
            file_to_image.TryGetValue(path, out img);
            if(img == null)
            {
                img = (Bitmap)Image.FromFile(path);
                file_to_image[path] = img;
            }
            BitmapData bd = null;
            bitmap_to_bitmap_data.TryGetValue(img, out bd);
            if(bd == null)
            {
                bd = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, img.PixelFormat);
                bitmap_to_bitmap_data[img] = bd;
            }
            tile_cache_mutex.ReleaseMutex();
            return bd;
        }

        public static void Dispose(string path)
        {
            tile_cache_mutex.WaitOne();
            Bitmap img = null;
            file_to_image.TryGetValue(path, out img);
            if(img != null)
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
        }

    }

    class Tile
    {
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
        /// The current date of the tile
        /// </summary>
        public DateTime TileDate;

        /// <summary>
        /// Disposes of this tile's image
        /// </summary>
        public void Dispose()
        {
            if(_image != null)
            {
                _image.Dispose();
                _image = null;
            }
        }

        public BitmapData GetImageCopy()
        {
            return TileCache.FetchImageData(TileFilePath);
        }

        public void DisposeCopy()
        {
            TileCache.Dispose(TileFilePath);
        }

        /// <summary>
        /// Used for rendering
        /// </summary>
        private Image _image;
        public Image Image
        {
            get
            {
                if(_image == null)
                {
                    _image = Image.FromFile(TileFilePath);
                }
                return _image;
            }
        }

    }
}
