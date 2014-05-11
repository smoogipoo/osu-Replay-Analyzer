using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace o_RA.Tables
{
    class BeatmapTag
    {
        public BeatmapTag()
        {
        }

        [UniqueIdentifier]
        public int? BeatmapTag_Id { get; set; }
        public string Name { get; set; }
    }
}
