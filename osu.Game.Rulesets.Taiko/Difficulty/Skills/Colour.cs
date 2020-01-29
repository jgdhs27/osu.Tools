// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Skills
{
    public class Colour : Skill
    {
        protected override double SkillMultiplier => 1;
        protected override double StrainDecayBase => 0.3;

        private ColourSwitch lastColourSwitch = ColourSwitch.None;
        private int sameColourCount = 1;

        protected override double StrainValueOf(DifficultyHitObject current)
        {


            double addition = 0;

            // We get an extra addition if we are not a slider or spinner
            if (current.LastObject is Hit && current.BaseObject is Hit && current.DeltaTime < 1000)
            {
                if (hasColourChange(current))
                    addition = 0.75;
            }
            else
            {
                lastColourSwitch = ColourSwitch.None;
                sameColourCount = 1;
            }

            return addition;
        }


        private bool hasColourChange(DifficultyHitObject current)
        {
            var taikoCurrent = (TaikoDifficultyHitObject) current;

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
