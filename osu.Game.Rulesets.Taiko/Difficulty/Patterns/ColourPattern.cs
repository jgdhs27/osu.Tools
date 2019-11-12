// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Patterns
{
    public class ColourPattern : Pattern
    {
        private const int int32_bits = 32;

        private readonly uint patternBitstring;

        public ColourPattern(IReadOnlyList<TaikoDifficultyHitObject> recentObjects, int length)
            : base(length)
        {
            Debug.Assert(length <= int32_bits, "Colour patterns are limited to 32 objects!");

            long b = 0;
            for (int i = 0; i < length; i++)
            {
                int nextBit = recentObjects[recentObjects.Count - length + i].BaseObject is RimHit ? 1 : 0;
                b = (patternBitstring << 1) ^ nextBit;
            }

            patternBitstring = (uint)b;
        }

        public override int GetHashCode() => (int)patternBitstring;

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType()) return false;

            ColourPattern c = (ColourPattern)obj;
            return c.patternBitstring == this.patternBitstring;
        };
    }
}
