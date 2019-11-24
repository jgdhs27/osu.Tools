// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Patterns
{
    public class PatternGenerator
    {


        public void Create(List<TaikoDifficultyHitObject> hitObjects)
        {

            List<RhythmPattern> rhythmPatterns = new List<RhythmPattern>();
            List<int> rhythmChangeLocations = new List<int>();

            for (int i = 0; i < hitObjects.Count(); i++)
            {
                if (hitObjects[i].HasTimingChange) rhythmChangeLocations.Add(i);
            }

            for (int i = 0; i < rhythmChangeLocations.Count; i++)
            {
                int patternStart = rhythmChangeLocations[i];
                for (int numChanges = 2; numChanges < 5; numChanges++)
                {
                    if (i + numChanges >= rhythmChangeLocations.Count) break;
                    int patternEnd = rhythmChangeLocations[i + numChanges];
                    RhythmPattern pattern = new RhythmPattern(hitObjects, patternStart, patternEnd-patternStart);
                    rhythmPatterns.Add(pattern);

                }
            }
        }

    }
}
