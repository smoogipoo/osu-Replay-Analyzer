using System;
using System.Collections.Generic;
using System.Diagnostics;
using BMAPI;

namespace MapInfoPlugin
{
    /// <summary>
    /// osu!tp's difficulty calculator ported to oRA.
    /// </summary>
    public class tpDifficulty
    {
        // Those values are used as array indices. Be careful when changing them!
        public enum DifficultyType
        {
            Speed = 0,
            Aim,
        };


        // We will store the HitObjects as a member variable.
        public List<tpHitObject> tpHitObjects;


        public const double STAR_SCALING_FACTOR = 0.045;
        public const double EXTREME_SCALING_FACTOR = 0.5;

        // Exceptions would be nicer to handle errors, but for this small project it shall be ignored.
        public bool CalculateStrainValues()
        {
            // Traverse hitObjects in pairs to calculate the strain value of NextHitObject from the strain value of CurrentHitObject and environment.
            List<tpHitObject>.Enumerator HitObjectsEnumerator = tpHitObjects.GetEnumerator();
            if (HitObjectsEnumerator.MoveNext() == false)
            {
                return false;
            }

            tpHitObject CurrentHitObject = HitObjectsEnumerator.Current;

            // First hitObject starts at strain 1. 1 is the default for strain values, so we don't need to set it here. See tpHitObject.

            while (HitObjectsEnumerator.MoveNext())
            {
                tpHitObject NextHitObject = HitObjectsEnumerator.Current;
                NextHitObject.CalculateStrains(CurrentHitObject);
                CurrentHitObject = NextHitObject;
            }

            return true;
        }


        // In milliseconds. For difficulty calculation we will only look at the highest strain value in each time interval of size STRAIN_STEP.
        // This is to eliminate higher influence of stream over aim by simply having more HitObjects with high strain.
        // The higher this value, the less strains there will be, indirectly giving long beatmaps an advantage.
        private const double STRAIN_STEP = 400;

        // The weighting of each strain value decays to 0.9 * it's previous value
        private const double DECAY_WEIGHT = 0.9;

        public double CalculateDifficulty(DifficultyType Type)
        {
            // Find the highest strain value within each strain step
            List<double> HighestStrains = new List<double>();
            double IntervalEndTime = STRAIN_STEP;
            double MaximumStrain = 0; // We need to keep track of the maximum strain in the current interval

            tpHitObject PreviousHitObject = null;
            foreach (tpHitObject hitObject in tpHitObjects)
            {
                // While we are beyond the current interval push the currently available maximum to our strain list
                while(hitObject.BaseHitObject.StartTime > IntervalEndTime)
                {
                    HighestStrains.Add(MaximumStrain);

                    // The maximum strain of the next interval is not zero by default! We need to take the last hitObject we encountered, take its strain and apply the decay
                    // until the beginning of the next interval.
                    if(PreviousHitObject == null)
                    {
                        MaximumStrain = 0;
                    }
                    else
                    {
                        double Decay = Math.Pow(tpHitObject.DECAY_BASE[(int)Type], (IntervalEndTime - PreviousHitObject.BaseHitObject.StartTime) / 1000);
                        MaximumStrain = PreviousHitObject.Strains[(int)Type] * Decay;
                    }

                    // Go to the next time interval
                    IntervalEndTime += STRAIN_STEP;
                }

                // Obtain maximum strain
                if (hitObject.Strains[(int)Type] > MaximumStrain)
                {
                    MaximumStrain = hitObject.Strains[(int)Type];
                }

                PreviousHitObject = hitObject;
            }

            // Build the weighted sum over the highest strains for each interval
            double Difficulty = 0;
            double Weight = 1;
            HighestStrains.Sort((a,b) => b.CompareTo(a)); // Sort from highest to lowest strain.

            foreach(double Strain in HighestStrains)
            {
                Difficulty += Weight * Strain;
                Weight *= DECAY_WEIGHT;
            }

            return Difficulty;
        }


    }
}
