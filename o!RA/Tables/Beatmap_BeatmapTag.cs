using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace o_RA.Tables
{
    class Beatmap_BeatmapTag
    {
        public Beatmap_BeatmapTag()
        {
        }

        [UniqueIdentifier]
        public int? Beatmap_BeatmapTag_Id { get; set; }
        public int? Beatmap_Id { get; set; }
        public int? BeatmapTag_Id { get; set; }
    }
}
