// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills {
    public class Tech : Skill
    {

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.3;

        private List<TaikoDifficultyHitObject> ratioObjectHistory = new List<TaikoDifficultyHitObject>();
        private int ratioHistoryLength = 0;
        private const int ratio_history_max_length = 8;
        protected override double StrainValueOf(DifficultyHitObject dho)
        {

            TaikoDifficultyHitObject currentHO = (TaikoDifficultyHitObject) dho;
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

    }
}
