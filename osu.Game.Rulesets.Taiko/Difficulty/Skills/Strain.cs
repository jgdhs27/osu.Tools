// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Strain : Skill
    {
        private const double rhythm_change_base_threshold = 0.2;
        private const double rhythm_change_base = 2.0;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.2;

        private ColourSwitch lastColourSwitch = ColourSwitch.None;

        private int sameColourCount = 1;

        protected override double StrainValueOf(DifficultyHitObject current)
        {

            double addition = 1;

            // We get an extra addition if we are not a slider or spinner
            if (current.LastObject is Hit && current.BaseObject is Hit && current.DeltaTime < 1000)
            {
                if (hasColourChange(current))
                    addition += 0.75;

                double rd = 0;

                if (hasRhythmChange(current))
                    rd = 1;



                //System.IO.File.AppendAllText(@"outold.txt", current.BaseObject.StartTime.ToString("0.000000") +
                //" " + current.BaseObject.ToString() + " " + rd.ToString("0.000000") + "\n");

                // rd = rhythmicDifficulty(current);

                rd = rhythmicDifficultyRatio((TaikoDifficultyHitObject)current);

                //System.IO.File.AppendAllText(@"outnew.txt", current.BaseObject.StartTime.ToString("0.000000") +
                //" " + current.BaseObject.ToString() + " " + rd.ToString("0.000000") + "\n");

                rd = 0.0;

                addition += rd;
            }
            else
            {
                lastColourSwitch = ColourSwitch.None;
                sameColourCount = 1;
            }

            double additionFactor = 1;

            // Scale the addition factor linearly from 0.4 to 1 for DeltaTime from 0 to 50
            if (current.DeltaTime < 50)
                additionFactor = 0.4 + 0.6 * current.DeltaTime / 50;

            // Console.WriteLine("additionFactor: {0:F20}", additionFactor * addition);

            // System.IO.File.AppendAllText(@"out.txt", current.BaseObject.StartTime.ToString("0.000000") +
            //         " " + current.BaseObject.ToString() + " " + (additionFactor * addition).ToString("0.000000") + "\n");

            return additionFactor * addition;
        }

        private List<HistoryObject> objectHistory = new List<HistoryObject>();

        private class HistoryObject
        {
            public static int LastRhythmID = 0;

            public int RhythmID;
            public double Time;
            public double DeltaTime;
            public HistoryObject(DifficultyHitObject o)
            {
                Time = o.BaseObject.StartTime;
                DeltaTime = o.DeltaTime;
            }
        }

        private bool sameWindow(HistoryObject o1, HistoryObject o2)
        {
            double timeElapsedRatio = Math.Max(o1.DeltaTime / o2.DeltaTime, o2.DeltaTime / o1.DeltaTime);

            return timeElapsedRatio < 1.05;
        }


        private List<TaikoDifficultyHitObject> ratioObjectHistory = new List<TaikoDifficultyHitObject>();
        private int ratioHistoryLength = 0;
        private const int ratio_history_max_length = 8;
        private double rhythmicDifficultyRatio(TaikoDifficultyHitObject currentHO)
        {

            if (!currentHO.HasTimingChange)
            {
                return 0.0;
            }

            double objectDifficulty = 1.0;

            ratioObjectHistory.Add(currentHO);
            // Console.WriteLine("timing change");
            ratioHistoryLength += 1;
            if (ratioHistoryLength > ratio_history_max_length)
            {
                ratioObjectHistory.RemoveAt(0);
                ratioHistoryLength -= 1;
            }

            // find repeated ratios

            for (int l = 2; l <= ratio_history_max_length / 2; l++)
            {
                for (int start = ratioHistoryLength - l - 1; start >= 0; start--)
                {
                    bool samePattern = true;
                    for (int i = 0; i < l; i++)
                    {
                        if (ratioObjectHistory[start + i].RhythmID != ratioObjectHistory[ratioHistoryLength - l + i].RhythmID)
                        {
                            samePattern = false;
                        }
                    }

                    if (samePattern) // Repitition found!
                    {
                        double timeSince = currentHO.BaseObject.StartTime - ratioObjectHistory[start].BaseObject.StartTime;
                        objectDifficulty *= Math.Atan(timeSince / 1000) / (Math.PI / 2);
                    }
                }
            }

            // objectDifficulty *= (50.0 / currentHO.DeltaTime);
            return objectDifficulty;

        }


        private double rhythmicDifficulty(DifficultyHitObject currentHO)
        {

            // how much of the history should be analysed, in ms
            int historyLength = 1000;

            HistoryObject current = new HistoryObject(currentHO);
            while (objectHistory.Count > 0 && objectHistory[0].Time < current.Time - historyLength)
            {
                objectHistory.RemoveAt(0);
            }

            double repititionValue = 1;

            HashSet<int> rhythmIDSet = new HashSet<int>();
            foreach (HistoryObject other in objectHistory)
            {
                if (sameWindow(current, other))
                {
                    current.RhythmID = other.RhythmID;
                    repititionValue *= Math.Sqrt((current.Time - other.Time) / historyLength);
                };
                rhythmIDSet.Add(other.RhythmID);
            }

            int uniqueRhythmCount = rhythmIDSet.Count;

            if (repititionValue == 1) // new rhythm
            {
                HistoryObject.LastRhythmID += 1;
                current.RhythmID = HistoryObject.LastRhythmID;
                uniqueRhythmCount += 1;
            }

            double uniqueBonus = Math.Sqrt(uniqueRhythmCount) / 2;

            objectHistory.Add(current);

            // the first two notes after a break have no rhythmic difficulty
            if (objectHistory.Count <= 2)
                return 0;

            return repititionValue * uniqueBonus;
        }

        private bool hasRhythmChange(DifficultyHitObject current)
        {
            // We don't want a division by zero if some random mapper decides to put two HitObjects at the same time.
            if (current.DeltaTime == 0 || Previous.Count == 0 || Previous[0].DeltaTime == 0)
                return false;

            double timeElapsedRatio = Math.Max(Previous[0].DeltaTime / current.DeltaTime, current.DeltaTime / Previous[0].DeltaTime);

            if (timeElapsedRatio >= 8)
                return false;

            double difference = Math.Log(timeElapsedRatio, rhythm_change_base) % 1.0;

            // rhythm_change_base = 2.0

            // return 0.2 < difference < 0.8
            return difference > rhythm_change_base_threshold && difference < 1 - rhythm_change_base_threshold;
        }

        private bool hasColourChange(DifficultyHitObject current)
        {
            var taikoCurrent = (TaikoDifficultyHitObject)current;

            if (!taikoCurrent.HasTypeChange)
            {
                sameColourCount++;
                return false;
            }

            var oldColourSwitch = lastColourSwitch;
            var newColourSwitch = sameColourCount % 2 == 0 ? ColourSwitch.Even : ColourSwitch.Odd;

            lastColourSwitch = newColourSwitch;
            sameColourCount = 1;

            // We only want a bonus if the parity of the color switch changes
            return oldColourSwitch != ColourSwitch.None && oldColourSwitch != newColourSwitch;
        }

        private enum ColourSwitch
        {
            None,
            Even,
            Odd
        }
    }
}
