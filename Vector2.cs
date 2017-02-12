using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalemMapper
{
    class Vector2i
    {

        public int X;
        public int Y;

        public Vector2i()
        {
            this.X = 0;
            this.Y = 0;
        }

        public Vector2i(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
        
        public static Vector2i operator +(Vector2i lhs, Vector2i rhs)
        {
            return new Vector2i(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }

        public static Vector2i operator -(Vector2i lhs, Vector2i rhs)
        {
            return new Vector2i(lhs.X - rhs.X, lhs.Y - rhs.Y);
        }

        public static Vector2i operator *(Vector2i lhs, Vector2i rhs)
        {
            return new Vector2i(lhs.X * rhs.X, lhs.Y * rhs.Y);
        }

    }
}
