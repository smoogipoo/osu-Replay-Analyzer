using System;
using System.Collections.Generic;

namespace BMAPI
{
    public class SliderInfo : BaseCircle
    {
        public SliderInfo()
        {
            Type = SliderType.Linear;
        }
        public SliderType Type { get; set; }
        public List<PointInfo> Points = new List<PointInfo>();
        public int RepeatCount { get; set; }
        public int EndTime { get; set; }
        public double MaxPoints { get; set; }

        public PointInfo PositionAtTime(int time)
        {
            //Todo
            if (Type == SliderType.Linear)
            {
                double dX = (Points[1].X - Points[0].X) / (EndTime - StartTime);
                double dY = (Points[1].Y - Points[0].Y) / (EndTime - StartTime);
            }
        }

        public override bool ContainsPoint(PointInfo Point)
        {
            return ContainsPoint(Point, 0);
        }
        public bool ContainsPoint(PointInfo Point, int Time)
        {
            PointInfo pAtTime = PositionAtTime(Time);
            return Math.Sqrt(Math.Pow(Point.X - pAtTime.X, 2) + Math.Pow(Point.Y - pAtTime.Y, 2)) <= Radius;            
        }
    }
}
