
using System;

namespace BMAPI
{
    public class PointInfo
    {
        public PointInfo(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public double X = 0;
        public double Y = 0;

        public double DistanceTo(PointInfo p2)
        {
            return Math.Sqrt(Math.Pow(p2.X - X, 2) + Math.Pow(p2.Y - Y, 2));
        }
        public static PointInfo operator *(PointInfo p, float multiple)
        {
            return new PointInfo(p.X * multiple, p.Y * multiple);
        }
    }
}
