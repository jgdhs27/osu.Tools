// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Patterns
{
    public abstract class Pattern
    {
        protected readonly int Length;
        protected readonly int patternStart;

        public abstract override int GetHashCode();
        public abstract override bool Equals(object obj);

        protected Pattern(int patternStart, int length)
        {
            this.patternStart = patternStart;
            this.Length = length;
        }

    }
}
