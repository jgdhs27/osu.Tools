// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyHitObject : DifficultyHitObject
    {
        public readonly bool HasTypeChange;

        public readonly int RhythmID;

        public TaikoDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject lastLastObject, double clockRate)
            : base(hitObject, lastObject, clockRate)
        {
            HasTypeChange = lastObject is RimHit != hitObject is RimHit;

            double thisLength = hitObject.StartTime - lastObject.StartTime;
            double prevLength = lastObject.StartTime - lastLastObject.StartTime;
            RhythmID = Rhythm.GetClosest(thisLength / prevLength).ID;
        }

        private class Rhythm
        {

            private static readonly int power_2_max = 4;
            private static readonly int power_3_max = 2;
            private static readonly double ratio_tol = Math.Pow(2, -power_2_max) * Math.Pow(3, -power_3_max) / 2;
            private static Rhythm[] commonRhythms;

            public int ID = 0;
            private readonly double ratio;
            private static void initialiseCommonRhythms()
            {
                List<Rhythm> commonRhythmList = new List<Rhythm>();
                for (int power2 = -4; power2 < 5; power2++)
                {
                    for (int power3 = -2; power3 < 3; power3++)
                    {
                        commonRhythmList.Add(new Rhythm(Math.Pow(2, power2) * Math.Pow(3, power3)));
                    }
                }
                commonRhythmList.Sort();
                double prevRatio = 0.0;
                int id = 0;
                List<Rhythm> uniques = new List<Rhythm>();
                foreach (Rhythm r in commonRhythmList)
                {
                    if (r.ratio - prevRatio > ratio_tol)
                    {
                        uniques.Append(r);
                        r.ID = id;
                        id++;
                    }
                    prevRatio = r.ratio;
                }

                commonRhythms = uniques.ToArray();
            }

            private Rhythm(double ratio)
            {
                this.ratio = ratio;
            }

            // Code is inefficient - we are searching exhaustively through the sorted list commonRhythms
            public static Rhythm GetClosest(double ratio)
            {
                if (commonRhythms == null)
                {
                    initialiseCommonRhythms();
                }

                Rhythm closestRhythm = commonRhythms[0];
                double closestDistance = Double.MaxValue;

                foreach (Rhythm r in commonRhythms)
                {
                    if (Math.Abs(r.ratio - ratio) < closestDistance)
                    {
                        closestRhythm = r;
                        closestDistance = Math.Abs(r.ratio - ratio);
                    }
                }

                return closestRhythm;

            }

        }
    }
}
