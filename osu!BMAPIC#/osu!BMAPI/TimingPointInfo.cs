
namespace BMAPI
{
    public class TimingPointInfo
    {
        public double Time = 0.0f;
        public double BpmDelay = 0.0f;
        public int TimeSignature = 4;
        public int SampleSet = 0;
        public int CustomSampleSet = 0;
        public int VolumePercentage = 100;
        public bool InheritsBPM = false;
        public bool KiaiTime = false;
        public bool OmitFirstBarLine = false;
    }
}
