using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace o_RA.Tables
{
    class Beatmap
    {
        public Beatmap()
        {
        }

        [UniqueIdentifier]
        public int? Beatmap_Id { get; set; }
        public string Creator { get; set; }
        public string AudioFilename { get; set; }
        public string Filename { get; set; }
        public decimal? HPDrainRate { get; set; }
        public decimal? CircleSize { get; set; }
        public decimal? OverallDifficulty { get; set; }
        public decimal? ApproachRate { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Version { get; set; }
    }
}
