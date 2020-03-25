// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing
{
    public class TaikoDifficultyHitObject : DifficultyHitObject
    {
        public readonly bool HasTypeChange;
        public readonly bool HasTimingChange;
        public readonly bool isBreak;
        public readonly TaikoDifficultyHitObjectRhythm Rhythm;

        public readonly int RhythmID;

        public readonly double NoteLength;

        public TaikoDifficultyHitObject(HitObject hitObject, HitObject lastObject, HitObject lastLastObject, double clockRate)
            : base(hitObject, lastObject, clockRate)
        {

            NoteLength = hitObject.StartTime - lastObject.StartTime;
            double prevLength = lastObject.StartTime - lastLastObject.StartTime;
            Rhythm = TaikoDifficultyHitObjectRhythm.GetClosest(NoteLength / prevLength);
            RhythmID = Rhythm.ID;
            HasTypeChange = lastObject is RimHit != hitObject is RimHit;
            HasTimingChange = !TaikoDifficultyHitObjectRhythm.IsRepeat(RhythmID);
        }

        public const int CONST_RHYTHM_ID = 0;


    }
}
