using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DerbyBracket.Model
{
    class RaceParameters
    {
        public ICollection<string> Racers { get; set; }

        public int ResultsToShow { get; set; }
    }
}
