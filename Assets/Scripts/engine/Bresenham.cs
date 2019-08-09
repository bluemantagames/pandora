// mostly from https://wiki.unity3d.com/index.php/Bresenham3D

using UnityEngine;
using System.Collections.Generic;

namespace Pandora.Engine
{
    public class Bresenham
    {
        static float steps = 1;

        public static IEnumerator<Vector2Int> GetEnumerator(Vector2Int start, Vector2Int end)
        {
            var result = new Vector2Int(0, 0);

            int xd, yd;
            int x, y;
            int ax, ay;
            int sx, sy;
            int dx, dy;

            dx = (int)(end.x - start.x);
            dy = (int)(end.y - start.y);

            ax = Mathf.Abs(dx) << 1;
            ay = Mathf.Abs(dy) << 1;

            sx = (int)Mathf.Sign((float)dx);
            sy = (int)Mathf.Sign((float)dy);

            x = (int)start.x;
            y = (int)start.y;

            if (ax >= ay) // x dominant
            {
                yd = ay - (ax >> 1);
                for (; ; )
                {
                    result.x = (int)(x / steps);
                    result.y = (int)(y / steps);

                    yield return result;

                    if (x == (int)end.x)
                        yield break;

                    if (yd >= 0)
                    {
                        y += sy;
                        yd -= ax;
                    }

                    x += sx;
                    yd += ay;
                }
            }
            else if (ay >= ax) // y dominant
            {
                xd = ax - (ay >> 1);

                for (; ; )
                {
                    result.x = (int)(x / steps);
                    result.y = (int)(y / steps);

                    yield return result;

                    if (y == (int)end.y)
                        yield break;

                    if (xd >= 0)
                    {
                        x += sx;
                        xd -= ay;
                    }

                    y += sy;
                    xd += ax;
                }
            }
        }
    }
}
