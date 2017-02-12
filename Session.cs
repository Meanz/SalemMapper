using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalemMapper
{
    class Session
    {
        private static int IMAGE_WIDTH = 100;
        private static int IMAGE_HEIGHT = 100;

        public string Name;
        public string Path;
        public List<Tile> Tiles;

        private Bitmap m_image;
        private Rectangle m_image_bounds;
        private int m_image_offset_x;
        private int m_image_offset_y;

        public void Render(Vector2i pos, Graphics graphics)
        {
            if(m_image == null)
            {
                //This seems kind of awkward, but yeah, need to know the size
                //T_T
                int min_x = int.MaxValue;
                int min_y = int.MaxValue;
                int max_x = int.MinValue;
                int max_y = int.MinValue;

                foreach (Tile t in Tiles)
                {
                    int t_x = t.LocalX * IMAGE_WIDTH;
                    int t_y = t.LocalY * IMAGE_WIDTH;
                    if (t_x < min_x) min_x = t_x;
                    if (t_x > max_x) max_x = t_x;
                    if (t_y < min_y) min_y = t_y;
                    if (t_y > max_y) max_y = t_y;
                }

                int width = max_x - min_x;
                int height = max_y - min_y;
                m_image_bounds = new Rectangle(min_x, min_y, width, height);

                // m_image = new Bitmap(width, height);
                // Graphics image_graphics = Graphics.FromImage(m_image);
                /*foreach (Tile t in Tiles)
                {
                    Image image = t.Image;
                    if (image != null)
                    {
                        image_graphics.DrawImage(image,
                            ((t.LocalX * IMAGE_WIDTH) + min_x),
                            ((t.LocalY * IMAGE_WIDTH) + min_y));
                    }
                }*/

            }

            foreach (Tile t in Tiles)
            {
                Image image = t.Image;
                if (image != null)
                {
                    graphics.DrawImage(image, 
                        pos.X + (t.LocalX * image.Width), 
                        pos.Y + (t.LocalY * image.Height));
                }
            }
          //  graphics.DrawImage(m_image, pos.X + m_image_offset_x, pos.Y + m_image_offset_y);
        }
    }
}
