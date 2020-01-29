// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Taiko.Difficulty.Preprocessing;

namespace osu.Game.Rulesets.Taiko.Difficulty.Patterns
{
    public class RhythmPattern : Pattern
    {

        private readonly int[] rhythmIDs;

        public bool Equals(int[] x, int[] y)
        {
            if (x.Length != y.Length)
            {
                return false;
            }
            for (int i = 0; i < x.Length; i++)
            {
                if (x[i] != y[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for (int i = 0; i < rhythmIDs.Length; i++)
            {
                unchecked // ignore int overflows
                {
                    hash = hash * 23 + rhythmIDs[i];
                }
            }
            return hash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType()) return false;

            RhythmPattern r = (RhythmPattern)obj;
            if (r.Length != this.Length) return false;

            for (int i = 0; i < this.Length; i++)
            {
                if (r.rhythmIDs[i] != this.rhythmIDs[i]) return false;
            }

            return true;

        }

        public RhythmPattern(IReadOnlyList<DifficultyHitObject> objects, int patternStart, int length)
            : base(patternStart, length)
        {
            rhythmIDs = new int[length];

            for (int i = 0; i < length; i++)
            {
                rhythmIDs[i] = ((TaikoDifficultyHitObject) objects[patternStart + i]).RhythmID;
            }
        }

        public override string ToString()
        {
            string s = "";

            for (int i = 0; i < Length; i++)
            {
                string note = rhythmIDs[i] + " ";
                s = s + note;
            }

            return s;
        }

    }
}
