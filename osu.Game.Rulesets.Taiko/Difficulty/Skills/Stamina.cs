// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Stamina : Skill
    {

        private int hand;
        private int noteNumber = 0;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.5;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            noteNumber += 1;
            noteNumber = noteNumber % 2;
            if (noteNumber == hand)
                return 1;
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
