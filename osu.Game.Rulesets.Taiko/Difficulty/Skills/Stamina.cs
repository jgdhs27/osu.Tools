// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Stamina : Skill
    {

        private TaikoDifficultyHitObject prevHO = null;
        private int monoCounter = 0;

        private int hand;
        private int noteNumber = 0;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.5;

        protected override double StrainValueOf(DifficultyHitObject current)
        {

            TaikoDifficultyHitObject currentHO = (TaikoDifficultyHitObject) current;

            noteNumber += 1;
            noteNumber = noteNumber % 2;

            if (noteNumber == hand)
            {


                if ((prevHO != null) && (currentHO.IsKat == prevHO.IsKat))
                {
                    monoCounter += 1;
                }
                else
                {
                    monoCounter = 1;
                }

                prevHO = currentHO;

                if (monoCounter > 2)
                {
                    return 1.2;
                }

                return 1.0;
            }

            return 0;
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
