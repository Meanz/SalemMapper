using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace SalemMapper
{
    class SessionAnalyzer
    {

        public static bool Matches(Session a, Session b)
        {
            return false;
        }

        public static bool Matches(Tile a, Tile b)
        {
            //Bitmap ia = (Bitmap)a.GetImageCopy();
           // Bitmap ib = (Bitmap)b.GetImageCopy();

            //Rectangle rect = new Rectangle(0, 0, ia.Width, ia.Height);

            // BitmapData bda = ia.LockBits(rect, ImageLockMode.ReadOnly, ia.PixelFormat);
            //BitmapData bdb = ib.LockBits(rect, ImageLockMode.ReadOnly, ib.PixelFormat);

            BitmapData bda = a.GetImageCopy();
            BitmapData bdb = b.GetImageCopy();

            int equality = 0;

            unsafe
            {
                byte* ptr1 = (byte*)bda.Scan0.ToPointer();
                byte* ptr2 = (byte*)bdb.Scan0.ToPointer();
                int width = bda.Width * 4;
                for(int y=0; y < bda.Height; y++)
                {
                    for(int x=0; x < bda.Width; x++)
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

           // ia.UnlockBits(bda);
           // ib.UnlockBits(bdb);

            double equality_perc = (equality / (double)(bda.Width * bda.Height)) * 100;
            if (equality_perc > 95.0d)
            {
                return true;
            }

           // ia.Dispose();
           // ib.Dispose();

            return false;

        }

    }
}
