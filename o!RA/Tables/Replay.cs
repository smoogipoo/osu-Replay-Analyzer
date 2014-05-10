using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace o_RA.Tables
{
    class Replay
    {
        public Replay()
        {
        }

        [UniqueIdentifier]
        public int? Replay_Id { get; set; }
        public int? GameMode { get; set; }
        public string Filename { get; set; }
        public string MapHash { get; set; }
        public string ReplayHash { get; set; }
        public string PlayerName { get; set; }
        public int? TotalScore { get; set; }
        public int? Count_300 { get; set; }
        public int? Count_100 { get; set; }
        public int? Count_50 { get; set; }
        public int? Count_Geki { get; set; }
        public int? Count_Katu { get; set; }
        public int? Count_Miss { get; set; }
        public int? MaxCombo { get; set; }
        public int? IsPerfect { get; set; }
        public DateTime? PlayTime { get; set; }
        public int? ReplayLength { get; set; }
    }
}
