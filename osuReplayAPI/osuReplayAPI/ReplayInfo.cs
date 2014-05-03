using System;

namespace ReplayAPI
{
    public class ReplayInfo
    {
        public Int64 TimeDiff;
        public int Time;
        public double TimeInSeconds {get { return Time/1000.0; }}
        public double X;
        public double Y;
        public KeyData Keys;
    }
}
