
using Microsoft.Xna.Framework;
namespace TempestGadgets.Utils
{


    public class CohenClip
    {
        // Cohen–Sutherland 裁剪常量
        const int INSIDE = 0; // 0000
        const int LEFT = 1;   // 0001
        const int RIGHT = 2;  // 0010
        const int BOTTOM = 4; // 0100
        const int TOP = 8;    // 1000

        static int ComputeOutCode(Rectangle rect, Vector2 p)
        {
            int code = INSIDE;
            if (p.X < rect.Left) code |= LEFT;
            else if (p.X > rect.Right) code |= RIGHT;
            if (p.Y < rect.Top) code |= TOP;
            else if (p.Y > rect.Bottom) code |= BOTTOM;
            return code;
        }

        public static bool CohenSutherlandClip(Rectangle rect, Vector2 p0, Vector2 p1,out Vector2 clippedP0, out Vector2 clippedP1)
        {
            clippedP0 = p0;
            clippedP1 = p1;
            int outcode0 = ComputeOutCode(rect, p0);
            int outcode1 = ComputeOutCode(rect, p1);

            bool accept = false;

            while (true)
            {
                if ((outcode0 | outcode1) == 0)
                {
                    // 全部在矩形内
                    accept = true;
                    break;
                }
                else if ((outcode0 & outcode1) != 0)
                {
                    // 全部在同一外侧
                    break;
                }
                else
                {
                    float x = 0, y = 0;
                    int outcodeOut = outcode0 != 0 ? outcode0 : outcode1;

                    if ((outcodeOut & TOP) != 0)
                    {
                        x = p0.X + (p1.X - p0.X) * (rect.Top - p0.Y) / (p1.Y - p0.Y);
                        y = rect.Top;
                    }
                    else if ((outcodeOut & BOTTOM) != 0)
                    {
                        x = p0.X + (p1.X - p0.X) * (rect.Bottom - p0.Y) / (p1.Y - p0.Y);
                        y = rect.Bottom;
                    }
                    else if ((outcodeOut & RIGHT) != 0)
                    {
                        y = p0.Y + (p1.Y - p0.Y) * (rect.Right - p0.X) / (p1.X - p0.X);
                        x = rect.Right;
                    }
                    else if ((outcodeOut & LEFT) != 0)
                    {
                        y = p0.Y + (p1.Y - p0.Y) * (rect.Left - p0.X) / (p1.X - p0.X);
                        x = rect.Left;
                    }

                    if (outcodeOut == outcode0)
                    {
                        p0 = new Vector2(x, y);
                        outcode0 = ComputeOutCode(rect, p0);
                    }
                    else
                    {
                        p1 = new Vector2(x, y);
                        outcode1 = ComputeOutCode(rect, p1);
                    }
                }
            }

            if (accept)
            {
                clippedP0 = p0;
                clippedP1 = p1;
                return true;
            }
            return false;
        }
    }
}
