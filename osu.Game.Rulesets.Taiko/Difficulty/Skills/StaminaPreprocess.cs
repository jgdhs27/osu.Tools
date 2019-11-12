// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class StaminaPreprocess : Skill
    {
        private const double rhythm_change_base_threshold = 0.2;
        private const double rhythm_change_base = 2.0;

        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.3;

        protected override double StrainValueOf(DifficultyHitObject current)
        {
            double additionFactor = 1;
            if (current.DeltaTime < 50)
                additionFactor = 0.4 + 0.6 * current.DeltaTime / 50;
            return additionFactor;
        }

    }
}
