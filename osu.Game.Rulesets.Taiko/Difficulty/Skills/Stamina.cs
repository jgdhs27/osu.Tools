﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Stamina : Skill
    {

        private int hand;
        private int noteNumber = 0;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.5;

        private readonly int maxHistoryLength = 2;
        private List<double> noteDurationHistory = new List<double>();
        private double offhandObjectDuration = Double.MaxValue;

        private static double shortNoteBonus(double duration)
        {
            // note that we are only looking at every 2nd note, so a 300bpm stream has a note duration of 100ms.
            if (duration >= 200) return 0;
            double d = 200 - duration;
            d *= d;
            d /= 120000;
            return d;
        }

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            noteNumber += 1;
            noteNumber = noteNumber % 2;

            TaikoDifficultyHitObject currentHO = (TaikoDifficultyHitObject) current;

            if (noteNumber == hand)
            {

                noteDurationHistory.Add(currentHO.NoteLength + offhandObjectDuration);

                if (noteDurationHistory.Count > maxHistoryLength)
                {
                    noteDurationHistory.RemoveAt(0);
                }

                double shortestRecentNote = min(noteDurationHistory);
                double bonus = shortNoteBonus(shortestRecentNote);

                return 1.0 + bonus;
            }

            offhandObjectDuration = currentHO.NoteLength;
            return 0;
        }

        private static double min(List<double> l)
        {
            double minimum = Double.MaxValue;

            foreach (double d in l)
            {
                if (d < minimum)
                {
                    minimum = d;
                }
            }

            return minimum;
        }

        public Stamina(bool rightHand)
        {
            hand = 0;
            if (rightHand)
            {
                hand = 1;
            }
        }

    }
}
