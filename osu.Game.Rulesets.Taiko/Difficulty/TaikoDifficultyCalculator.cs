// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Skills;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty
{
    public class TaikoDifficultyCalculator : DifficultyCalculator
    {
        public TaikoDifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        private DifficultyAttributes taikoCalculate(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            var skills = CreateSkills(beatmap);

            if (!beatmap.HitObjects.Any())
                return CreateDifficultyAttributes(beatmap, mods, skills, clockRate);

            var difficultyHitObjects = CreateDifficultyHitObjects(beatmap, clockRate).OrderBy(h => h.BaseObject.StartTime).ToList();

            double sectionLength = SectionLength * clockRate;

            // The first object doesn't generate a strain, so we begin with an incremented section end
            double currentSectionEnd = Math.Ceiling(beatmap.HitObjects.First().StartTime / sectionLength) * sectionLength;

            foreach (DifficultyHitObject h in difficultyHitObjects)
            {
                while (h.BaseObject.StartTime > currentSectionEnd)
                {
                    foreach (Skill s in skills)
                    {
                        s.SaveCurrentPeak();
                        s.StartNewSectionFrom(currentSectionEnd);
                    }

                    currentSectionEnd += sectionLength;
                }

                foreach (Skill s in skills)
                    s.Process(h);
            }

            // The peak strain will not be saved for the last section in the above loop
            foreach (Skill s in skills)
                s.SaveCurrentPeak();

            return CreateDifficultyAttributes(beatmap, mods, skills, clockRate);
        }

        private double norm(double p, double v1, double v2, double v3)
        {
            return Math.Pow(
                Math.Pow(v1, p) +
                Math.Pow(v2, p) +
                Math.Pow(v3, p)
                , 1 / p);
        }

        private double combinedDifficulty(Skill colour, Skill rhythm, Skill stamina1, Skill stamina2)
        {
            double difficulty = 0;
            double weight = 1;
            List<double> peaks = new List<double>();
            for (int i = 0; i < colour.StrainPeaks.Count; i++)
            {
                double colourPeak = Math.Pow(colour.StrainPeaks[i], 0.8) * 0.0;
                double rhythmPeak = Math.Pow(rhythm.StrainPeaks[i], 1.5) * 0.025;
                double staminaPeak = Math.Pow(stamina1.StrainPeaks[i] + stamina2.StrainPeaks[i], 1.5) * 0.0017;
                peaks.Add(norm(3, colourPeak, rhythmPeak, staminaPeak));
            }
            foreach (double strain in peaks.OrderByDescending(d => d))
            {
                difficulty += strain * weight;
                weight *= 0.9;
            }

            return Math.Pow(difficulty, 0.9) * 3.8;
        }

        protected override DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate)
        {
            if (beatmap.HitObjects.Count == 0)
                return new TaikoDifficultyAttributes { Mods = mods, Skills = skills };

            double colourRating = Math.Pow(skills[0].DifficultyValue(), 0.8) * 0.05;
            double rhythmRating = Math.Pow(skills[1].DifficultyValue(), 1.5) * 0.025;
            double staminaRating = Math.Pow(skills[2].DifficultyValue() + skills[3].DifficultyValue(), 1.5) * 0.0017;
            double combinedRating = combinedDifficulty(skills[0], skills[1], skills[2], skills[3]);

            Console.WriteLine("colour\t" + colourRating);
            Console.WriteLine("rhythm\t" + rhythmRating);
            Console.WriteLine("stamina\t" + staminaRating);
            double starRating = norm(2, colourRating, rhythmRating, staminaRating);

            return new TaikoDifficultyAttributes
            {
                StarRating = combinedRating,
                Mods = mods,
                // Todo: This int cast is temporary to achieve 1:1 results with osu!stable, and should be removed in the future
                GreatHitWindow = (int)(beatmap.HitObjects.First().HitWindows.Great / 2) / clockRate,
                MaxCombo = beatmap.HitObjects.Count(h => h is Hit),
                Skills = skills
            };
        }

        protected override IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate)
        {
            for (int i = 2; i < beatmap.HitObjects.Count; i++)
                yield return new TaikoDifficultyHitObject(beatmap.HitObjects[i], beatmap.HitObjects[i - 1], beatmap.HitObjects[i - 2], clockRate);
        }

        protected override Skill[] CreateSkills(IBeatmap beatmap) => new Skill[]
        {
            new Colour(),
            new Rhythm(),
            new Stamina(true),
            new Stamina(false),
        };

        protected override Mod[] DifficultyAdjustmentMods => new Mod[]
        {
            new TaikoModDoubleTime(),
            new TaikoModHalfTime(),
            new TaikoModEasy(),
            new TaikoModHardRock(),
        };

        protected override DifficultyAttributes VirtualCalculate(IBeatmap beatmap, Mod[] mods, double clockRate)
            => taikoCalculate(beatmap, mods, clockRate);

    }
}
