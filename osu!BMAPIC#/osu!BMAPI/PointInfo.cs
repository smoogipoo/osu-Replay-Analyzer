
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

        public double DistanceTo(PointInfo P)
        {
            return Math.Sqrt(Math.Pow(P.X - X, 2) + Math.Pow(P.Y - Y, 2));
        }
        public static PointInfo operator *(PointInfo P, float Value)
        {
            return new PointInfo(P.X * Value, P.Y * Value);
        }
        public static PointInfo operator -(PointInfo P1, PointInfo P2)
        {
            return new PointInfo(P1.X - P2.X, P1.Y - P2.Y);
        }
    }
}
