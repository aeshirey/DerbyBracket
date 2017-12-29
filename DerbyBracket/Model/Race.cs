using System.Collections.Generic;

namespace DerbyBracket.Model
{
    public class Race
    {
        public int RaceNumber { get; set; }

        public IList<RaceIndividual> Racers { get; set; }
        

        public string Racer1 { get { return Racers[0].Racer; } }
        public string Racer2 { get { return Racers[1].Racer; } }
        public string Racer3 { get { return Racers[2].Racer; } }
        public string Racer4 { get { return Racers[3].Racer; } }
    }
}
