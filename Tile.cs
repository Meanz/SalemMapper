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
        /// 
        /// </summary>
        public Session Session;

        /// <summary>
        /// 
        /// </summary>
        private string _cachedFilePath;
        public string TileFilePath
        {
            get
            {
                if(_cachedFilePath == null)
                {
                    _cachedFilePath = Session.Path + "\\tile_" + LocalX + "_" + LocalY + ".png";
                }
                return _cachedFilePath;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public BitmapData GetBitmapData()
        {
            return TileCache.FetchImageData(TileFilePath);
        }

    }
}
