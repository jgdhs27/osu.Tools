// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyHitObjectRhythm
    {

        private static TaikoDifficultyHitObjectRhythm[] commonRhythms;
        private static TaikoDifficultyHitObjectRhythm constRhythm;
        private static int constRhythmID;

        public int ID = 0;
        public readonly double Difficulty;
        private readonly double ratio;

        private static void initialiseCommonRhythms()
        {


            /*
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
            */

            /*

                ALCHYRS CODE

            If (change < 0.48) Then 'sometimes gaps are slightly different due to position rounding
                Return 0.65 'This number increases value of anything that more than doubles speed. Affects doubles.
            ElseIf (change < 0.52) Then
                Return 0.5 'speed doubling - this one affects pretty much every map other than stream maps
            ElseIf change <= 0.9 Then
                Return 1.0 'This number increases value of 1/4 -> 1/6 and other weird rhythms.
            ElseIf change < 0.95 Then
                Return 0.25 '.9
            ElseIf change > 1.95 Then
                Return 0.3 'half speed or more - this affects pretty much every map
            ElseIf change > 1.15 Then
                Return 0.425 'in between - this affects (mostly) 1/6 -> 1/4
            ElseIf change > 1.05 Then
                Return 0.15 '.9, small speed changes

            */


            commonRhythms = new TaikoDifficultyHitObjectRhythm[]
            {
                new TaikoDifficultyHitObjectRhythm(1, 1, 0.1),
                new TaikoDifficultyHitObjectRhythm(2, 1, 0.3),
                new TaikoDifficultyHitObjectRhythm(1, 2, 0.5),
                new TaikoDifficultyHitObjectRhythm(3, 1, 0.3),
                new TaikoDifficultyHitObjectRhythm(1, 3, 0.35),
                new TaikoDifficultyHitObjectRhythm(3, 2, 1.0),
                new TaikoDifficultyHitObjectRhythm(2, 3, 1.0),
                new TaikoDifficultyHitObjectRhythm(5, 4, 1.0),
                new TaikoDifficultyHitObjectRhythm(4, 5, 1.0)
            };

            for (int i = 0; i < commonRhythms.Length; i++)
            {
                commonRhythms[i].ID = i;
            }

            constRhythmID = 0;
            constRhythm = commonRhythms[constRhythmID];

        }

        public bool IsRepeat()
        {
            return ID == constRhythmID;
        }

        public static bool IsRepeat(int id)
        {
            return id == constRhythmID;
        }

        public bool IsSpeedup()
        {
            return ratio < 1.0;
        }

        private TaikoDifficultyHitObjectRhythm(double ratio, double difficulty)
        {
            this.ratio = ratio;
            this.Difficulty = difficulty;
        }

        private TaikoDifficultyHitObjectRhythm(int numerator, int denominator, double difficulty)
        {
            this.ratio = ((double)numerator) / ((double)denominator);
            this.Difficulty = difficulty;
        }

        // Code is inefficient - we are searching exhaustively through the sorted list commonRhythms
        public static TaikoDifficultyHitObjectRhythm GetClosest(double ratio)
        {
            if (commonRhythms == null)
            {
                initialiseCommonRhythms();
            }

            TaikoDifficultyHitObjectRhythm closestRhythm = commonRhythms[0];
            double closestDistance = Double.MaxValue;

            foreach (TaikoDifficultyHitObjectRhythm r in commonRhythms)
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
