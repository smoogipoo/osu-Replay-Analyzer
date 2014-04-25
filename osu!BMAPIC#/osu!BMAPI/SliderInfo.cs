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
        public double MaxPoints { get; set; }
    }
}
