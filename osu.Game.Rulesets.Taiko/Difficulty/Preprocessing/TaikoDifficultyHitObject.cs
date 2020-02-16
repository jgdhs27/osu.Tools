// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyHitObject : DifficultyHitObject
    {
        public readonly bool HasTypeChange;
        public readonly bool HasTimingChange;
        public readonly bool isBreak;

        public readonly int RhythmID;

        public readonly double noteLength;

        public TaikoDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject lastLastObject, double clockRate)
            : base(hitObject, lastObject, clockRate)
        {

            noteLength = hitObject.StartTime - lastObject.StartTime;
            double prevLength = lastObject.StartTime - lastLastObject.StartTime;
            RhythmID = Rhythm.GetClosest(noteLength / prevLength).ID;

            HasTypeChange = lastObject is RimHit != hitObject is RimHit;
            HasTimingChange = !Rhythm.IsRepeat(RhythmID);
        }

        public const int CONST_RHYTHM_ID = 0;

        private class Rhythm
        {

            private static Rhythm[] commonRhythms;
            private static Rhythm constRhythm;

            public int ID = 0;
            private readonly double ratio;
            private static void initialiseCommonRhythms()
            {
                List<Rhythm> commonRhythmList = new List<Rhythm>();
                commonRhythmList.Add(new Rhythm(1, 1));
                for (int p2 = -2; p2 < 3; p2++)
                {
                    for (int p3 = -1; p3 < 2; p3++)
                    {
                        for (int p5 = -1; p5 < 1; p5++)
                        {
                            double r = Math.Pow(2, p2) * Math.Pow(3, p3) * Math.Pow(5, p5);
                            if (r != 1.0 && r >= 0.5 && r <= 2)
                            {
                                commonRhythmList.Add(new Rhythm(r));
                            }
                        }
                    }

                }
                commonRhythmList.Add(new Rhythm(9/4));
                commonRhythmList.Add(new Rhythm(4/9));


                commonRhythms = commonRhythmList.ToArray();

                /*
                commonRhythms = new Rhythm[]
                {
                    new Rhythm(1, 1),
                    new Rhythm(2, 1),
                    new Rhythm(1, 2),
                    new Rhythm(3, 1),
                    new Rhythm(1, 3),
                    new Rhythm(3, 2),
                    new Rhythm(2, 3)
                };
                */

                for (int i = 0; i < commonRhythms.Length; i++)
                {
                    commonRhythms[i].ID = i;
                }

                constRhythm = commonRhythms[CONST_RHYTHM_ID];

            }

            public static bool IsRepeat(int id)
            {
                return id == CONST_RHYTHM_ID;
            }

            private Rhythm(double ratio)
            {
                this.ratio = ratio;
            }

            private Rhythm(int numerator, int denominator)
            {
                this.ratio = ((double)numerator) / ((double)denominator);
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
