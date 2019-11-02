// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills {
    public class Tech : Skill
    {

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.3;

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

        protected override double StrainValueOf(DifficultyHitObject current)
        {

            // We get an extra addition if we are not a slider or spinner
            if (!(current.LastObject is Hit) || !(current.BaseObject is Hit) || !(current.DeltaTime < 1000))
            {
                return 0;
            }

            // how much of the history should be analysed, in ms
            int historyLength = 1000;

            HistoryObject currentHO = new HistoryObject(current);

            while (objectHistory.Count > 0 && objectHistory[0].Time < currentHO.Time - historyLength)
            {
                objectHistory.RemoveAt(0);
            }

            double repititionValue = 1;

            HashSet<int> rhythmIDSet = new HashSet<int>();
            foreach (HistoryObject other in objectHistory)
            {
                if (sameWindow(currentHO, other))
                {
                    currentHO.RhythmID = other.RhythmID;
                    repititionValue *= Math.Sqrt((currentHO.Time - other.Time) / historyLength);
                };
                rhythmIDSet.Add(other.RhythmID);
            }

            int uniqueRhythmCount = rhythmIDSet.Count;

            if (repititionValue == 1) // new rhythm
            {
                HistoryObject.LastRhythmID += 1;
                currentHO.RhythmID = HistoryObject.LastRhythmID;
                uniqueRhythmCount += 1;
            }

            double uniqueBonus = Math.Sqrt(uniqueRhythmCount) / 2;

            objectHistory.Add(current);

            // the first two notes after a break have no rhythmic difficulty
            if (objectHistory.Count <= 2)
                return 0;

            double rd = repititionValue * uniqueBonus;

            double additionFactor = 1;

            if (current.DeltaTime < 50)
                additionFactor = 0.4 + 0.6 * current.DeltaTime / 50;

            return additionFactor * rd;
        }

}
}
