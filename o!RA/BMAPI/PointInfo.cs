
using System;

namespace BMAPI
{
    public class PointInfo
    {
        public PointInfo(double X, double Y)
        {
            x = X;
            y = Y;
        }

        public double x = 0;
        public double y = 0;

        public double DistanceTo(PointInfo p2)
        {
            return Math.Sqrt(Math.Pow(p2.x - x, 2) + Math.Pow(p2.y - y, 2));
        }
    }
}
