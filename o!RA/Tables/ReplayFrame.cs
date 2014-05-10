using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace o_RA.Tables
{
    class ReplayFrame
    {
        public ReplayFrame()
        {

        }

        [UniqueIdentifier]
        public int? ReplayFrame_Id { get; set; }
        public Int64? TimeDiff { get; set; }
        public int? Time { get; set; }
        public decimal? X { get; set; }
        public decimal? Y { get; set; }
        public int? Replay_Id { get; set; }
    }
}
