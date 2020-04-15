// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills {
    public class Rhythm : Skill
    {

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.6;

        private List<TaikoDifficultyHitObject> ratioObjectHistory = new List<TaikoDifficultyHitObject>();
        private int ratioHistoryLength = 0;
        private const int ratio_history_max_length = 8;

        private int rhythmLength = 0;

        // Penalty for repeated sequences of rhythm changes
        private double repititionPenalty(double timeSinceRepititionMS)
        {
            double t = Math.Atan(timeSinceRepititionMS / 1000) / (Math.PI / 2);
            return t;
        }

        // Penalty for short patterns
        private double patternLengthPenalty(int patternLength)
        {
            return Math.Min(0.15 * patternLength, 1.0);
        }

        // Penalty for notes so slow that alting is not necessary.
        private double speedPenalty(double noteLengthMS)
        {
            if (noteLengthMS < 80) return 1;
            else if (noteLengthMS > 160) return 0.6;
            else return (-0.005 * noteLengthMS + 1.4);
        }

        // Penalty for the first rhythm change in a pattern
        private const double first_burst_penalty = 0.3;
        private bool prevIsSpeedup = true;

        protected override double StrainValueOf(DifficultyHitObject dho)
        {
            TaikoDifficultyHitObject currentHO = (TaikoDifficultyHitObject) dho;
            rhythmLength += 1;
            if (!currentHO.HasTimingChange)
            {
                return 0.0;
            }

            double objectDifficulty = currentHO.Rhythm.Difficulty;

            ratioObjectHistory.Add(currentHO);
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
                        objectDifficulty *= repititionPenalty(timeSince);
                        break;
                    }
                }
            }

            if (currentHO.Rhythm.IsSpeedup())
            {
                if (prevIsSpeedup)
                {
                    objectDifficulty *= first_burst_penalty;
                }

                prevIsSpeedup = true;
            }

            objectDifficulty *= patternLengthPenalty(rhythmLength);
            objectDifficulty *= speedPenalty(currentHO.NoteLength);

            rhythmLength = 0;

            return objectDifficulty;

        }

    }
}
