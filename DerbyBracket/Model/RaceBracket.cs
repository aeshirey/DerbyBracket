using System;
using System.Collections.Generic;
using System.Linq;

namespace DerbyBracket.Model
{
    public class RaceBracket
    {
        public ICollection<Race> Races { get; set; }

        public RaceBracket() { }

        /// <summary>
        /// Randomly generate a bracket based on the set of <see cref="racers"/>
        /// </summary>
        /// <param name="racers">The names of each racer</param>
        /// <param name="laneCount">The number of lanes</param>
        public RaceBracket(string[] racers, int laneCount)
        {
            Random Random = new Random();

            // Start off by taking the list of racers, [A, B, C, D, E, F, G, H], and adding the first
            // three to the end: [A, B, C, D, E, F, G, H, A, B, C]
            // let the races wrap around
            var racersEx = racers
                .Concat(racers.Take(laneCount - 1))
                .ToList();

            // Now we can easily generate a list of admittedly boring races:
            // [A, B, C, D]
            // [B, C, D, E]
            // [C, D, E, F]
            // ...
            // [H, A, B, C]
            // This ensures that each racer appears exactly once in each lane.
            IList<Race> races = new List<Race>();
            for (var i = 0; i <= racersEx.Count - laneCount; i++)
            {
                races.Add(new Race
                {
                    RaceNumber = i + 1,
                    Racers = new List<RaceIndividual>
                      {
                           new RaceIndividual { Lane = 1, Racer = racersEx[i] },
                           new RaceIndividual { Lane = 2, Racer = racersEx[i+1] },
                           new RaceIndividual { Lane = 3, Racer = racersEx[i+2] },
                           new RaceIndividual { Lane = 4, Racer = racersEx[i+3] }
                      }
                });
            }

            // Unfortunately, it also means that each racer is limited to races with a small set of
            // people. Not a problem mathematically, but it's not as exciting. To combat this, we
            // randomly swap racers around. First, by shuffling these boring races:
            races = races
                .OrderBy(race => Random.Next())
                .ToList();

            // Then, for any two sequential races, attempt to randomly swap racers *in the same lane*
            // that aren't in the other race. For example, [A, B, C, D] and [E, F, G, H] -- we could
            // turn these two independent races into [E, B, C, D] and [A, F, G, H], swapping A & E.
            // Or you could swpa B & F for [A, F, C, D] and [E, B, G, H]. Or you could do multiple
            // swaps between them.
            for (var i = 0; i < races.Count - 1; i++)
            {
                Race r1 = races[i],
                    r2 = races[i + 1];

                foreach (var j in PossibleSwaps(r1, r2))
                {
                    if (Random.NextDouble() > 0.5)
                    {
                        var tmp = r1.Racers[j];
                        r1.Racers[j] = r2.Racers[j];
                        r2.Racers[j] = tmp;
                    }
                }
            }

            // Hopefully, we've given some randomization to the races. More racers gives more chances
            // that this has happened. Now, return the races that we've organized.
            this.Races = races
                    .OrderBy(r => r.RaceNumber)
                    .ToList();
        }


        /// <summary>
        /// Given two races, figure out which positions could possibly be swapped without 
        /// creating any conflicts (eg, the same race containing one racer twice)
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        private IEnumerable<int> PossibleSwaps(Race r1, Race r2)
        {
            var names1 = r1.Racers.Select(i => i.Racer).ToList();
            var names2 = r2.Racers.Select(i => i.Racer).ToList();

            return Enumerable.Range(0, names1.Count)
                .Where(i =>
                {
                    string x = names1[i], y = names2[i];
                    return !names2.Contains(x) && !names1.Contains(y);
                })
                .Distinct()
                .OrderBy(i => i);
        }
    }
}
