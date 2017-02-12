using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalemMapper
{
    class RenderTarget
    {
        private int m_width;
        public int Width { get { return m_width; } }
        private int m_height;
        public int Height { get { return m_height; } }

        private Bitmap m_image;
        private Graphics m_graphics;

        /// <summary>
        /// Construct a new render target
        /// </summary>
        public RenderTarget()
        {
            m_width = 0;
            m_height = 0;
            m_image = null;
            m_graphics = null;
        }

        /// <summary>
        /// Resize this render target
        /// </summary>
        /// <param name="new_width"></param>
        /// <param name="new_height"></param>
        public void Resize(int new_width, int new_height)
        {
            if(m_image != null)
            {
                m_image.Dispose();
            }
            if (m_graphics != null)
            {
                m_graphics.Dispose();
            }
            m_image = new Bitmap(new_width, new_height);
            m_graphics = Graphics.FromImage(m_image);
        }

        /// <summary>
        /// Clears this render target
        /// </summary>
        public void Clear()
        {
            if(m_graphics != null)
            {
                m_graphics.FillRectangle(Brushes.Black, 0, 0, Width, Height);
            }
        }

        /// <summary>
        /// Draw an image on this render target
        /// </summary>
        /// <param name="image"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void DrawImage(Image image, int x, int y)
        {
            if(m_graphics != null)
            {
                m_graphics.DrawImage(image, x, y);
            }
        }

        /// <summary>
        /// Renders this render target onto the given graphics context
        /// </summary>
        /// <param name="graphics"></param>
        public void Render(Graphics graphics)
        {
            if(m_image != null)
            {
                graphics.DrawImage(m_image, 0, 0);
            }
        }

    }
}
