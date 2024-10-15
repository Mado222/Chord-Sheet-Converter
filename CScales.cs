using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChordSheetConverter
{
    public static class CScales
    {        // All possible notes for easy transposition
        public static readonly List<string> chromaticScale = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];

        public enum ScaleType
        {
            // Major and Minor scales
            Major,
            NaturalMinor,
            HarmonicMinor,
            MelodicMinor,

            // Modes derived from the Major scale
            Ionian,          // Same as Major
            Dorian,
            Phrygian,
            Lydian,
            Mixolydian,
            Aeolian,         // Same as Natural Minor
            Locrian,

            // Pentatonic scales
            MajorPentatonic,
            MinorPentatonic,

            // Other common scales
            BluesScale,
            ChromaticScale,
            WholeToneScale,
            HalfWholeDiminished,
            WholeHalfDiminished,
            AugmentedScale,

            // Exotic scales
            SpanishPhrygian,
            HungarianMinor,
            JapaneseScale
        }

        private static readonly Dictionary<ScaleType, List<string>> scaleIntervals = new()
{
    // Major and Minor scales
    { ScaleType.Major, new List<string> { "C", "D", "E", "F", "G", "A", "B" } },
    { ScaleType.NaturalMinor, new List<string> { "A", "B", "C", "D", "E", "F", "G" } },
    { ScaleType.HarmonicMinor, new List<string> { "A", "B", "C", "D", "E", "F", "G#" } },
    { ScaleType.MelodicMinor, new List<string> { "A", "B", "C", "D", "E", "F#", "G#" } },

    // Modes derived from the Major scale
    { ScaleType.Ionian, new List<string> { "C", "D", "E", "F", "G", "A", "B" } },          // Same as Major
    { ScaleType.Dorian, new List<string> { "D", "E", "F", "G", "A", "B", "C" } },
    { ScaleType.Phrygian, new List<string> { "E", "F", "G", "A", "B", "C", "D" } },
    { ScaleType.Lydian, new List<string> { "F", "G", "A", "B", "C", "D", "E" } },
    { ScaleType.Mixolydian, new List<string> { "G", "A", "B", "C", "D", "E", "F" } },
    { ScaleType.Aeolian, new List<string> { "A", "B", "C", "D", "E", "F", "G" } },         // Same as Natural Minor
    { ScaleType.Locrian, new List<string> { "B", "C", "D", "E", "F", "G", "A" } },

    // Pentatonic scales
    { ScaleType.MajorPentatonic, new List<string> { "C", "D", "E", "G", "A" } },
    { ScaleType.MinorPentatonic, new List<string> { "A", "C", "D", "E", "G" } },

    // Other common scales
    { ScaleType.BluesScale, new List<string> { "A", "C", "D", "D#", "E", "G" } },
    { ScaleType.ChromaticScale, new List<string> { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" } },
    { ScaleType.WholeToneScale, new List<string> { "C", "D", "E", "F#", "G#", "A#" } },
    { ScaleType.HalfWholeDiminished, new List<string> { "C", "D♭", "E♭", "E", "F#", "G", "A", "B♭" } },
    { ScaleType.WholeHalfDiminished, new List<string> { "C", "D", "E♭", "F", "F#", "G#", "A", "B" } },
    { ScaleType.AugmentedScale, new List<string> { "C", "E", "G#", "B", "C#" } },

    // Exotic scales
    { ScaleType.SpanishPhrygian, new List<string> { "C", "D♭", "E", "F", "G", "A♭", "B♭" } },
    { ScaleType.HungarianMinor, new List<string> { "C", "D", "E♭", "F#", "G", "A♭", "B" } },
    { ScaleType.JapaneseScale, new List<string> { "C", "D", "E♭", "G", "A♭" } }
};

        // Function to get the notes of a scale given its ScaleType and root note
        public static List<string> GetScaleNotes(ScaleType scaleType, string rootNote)
        {
            // Get the scale degrees from the dictionary
            if (!scaleIntervals.ContainsKey(scaleType))
            {
                throw new ArgumentException($"Invalid scale type: {scaleType}");
            }
            List<string> scaleDegrees = scaleIntervals[scaleType];

            // Find the position of the root note in the chromatic scale
            int rootIndex = chromaticScale.IndexOf(rootNote);
            if (rootIndex == -1)
            {
                throw new ArgumentException($"Invalid root note: {rootNote}");
            }

            // Transpose the scale degrees based on the root note
            List<string> transposedScale = [];
            foreach (string note in scaleDegrees)
            {
                // Find the interval for each note relative to C (the base of the scale)
                int interval = chromaticScale.IndexOf(note) - chromaticScale.IndexOf("C");
                int transposedIndex = (rootIndex + interval + chromaticScale.Count) % chromaticScale.Count;
                transposedScale.Add(chromaticScale[transposedIndex]);
            }

            return transposedScale;
        }
    }
}
