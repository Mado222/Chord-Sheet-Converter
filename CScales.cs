using System.Text.RegularExpressions;

namespace ChordSheetConverter
{
    public static partial class CScales
    {        // All possible notes for easy transposition
        public static readonly List<string> chromaticScale = ["C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"];
        // Define the Nashville number system scale, with "1" being the root note
        public static readonly List<string> nashvilleScale = [ "1", "2", "3", "4", "5", "6", "7" ];


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
            if (!scaleIntervals.TryGetValue(scaleType, out List<string>? value))
            {
                throw new ArgumentException($"Invalid scale type: {scaleType}");
            }
            List<string> scaleDegrees = value;

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
        public static bool IsValidChord(string chord)
        {
            string chordPattern = @"^[A-G][#b]?m?(maj|sus|dim|aug)?[0-9]?(add[0-9])?(/[A-G][#b]?)?$";
            return Regex.IsMatch(chord, chordPattern);
        }

        [GeneratedRegex(@"^[A-G][#b]?")]
        private static partial Regex RegexConvertChordToNashville();

        public static string ConvertChordToNashville(string chord, string key, ScaleType scaleType = ScaleType.Major)
        {
            // Get the scale corresponding to the provided ScaleType and key
            if (!scaleIntervals.TryGetValue(scaleType, out List<string>? scaleNotes))
            {
                throw new ArgumentException($"Invalid scale type: {scaleType}");
            }

            // Find the index of the root note (key) in the chromatic scale
            int keyIndex = chromaticScale.IndexOf(key);
            if (keyIndex == -1)
            {
                throw new ArgumentException($"Invalid key: {key}");
            }

            // Transpose the scale based on the key to get the scale in the correct key
            List<string> transposedScale = [];
            for (int i = 0; i < scaleNotes.Count; i++)
            {
                int noteIndex = (chromaticScale.IndexOf(scaleNotes[i]) + keyIndex) % chromaticScale.Count;
                transposedScale.Add(chromaticScale[noteIndex]);
            }

            // Extract the root note from the chord (e.g., C from Cmaj7)
            string rootNote = RegexConvertChordToNashville().Match(chord).Value;
            string chordSuffix = chord[rootNote.Length..];  // Extract the rest of the chord (e.g., maj7, m7, etc.)

            // Find the position of the root note in the transposed scale
            int chordIndex = transposedScale.IndexOf(rootNote);
            if (chordIndex == -1)
            {
                throw new ArgumentException($"Invalid chord root: {rootNote}");
            }

            // Nashville notation uses 1-based indexing for the scale degrees
            string nashvilleNumber = (chordIndex + 1).ToString();

            // Return the Nashville notation along with the chord suffix (e.g., "1m", "4maj7")
            return nashvilleNumber + chordSuffix;
        }

        public static string Transpose(string chord, int steps)
        {
            // Pattern to detect letter notation chords (e.g., "G#m7", "F#maj7", "Cadd9")
            string letterChordPattern = @"^[A-G][#b]?";

            // Pattern to detect Nashville notation (e.g., "1", "4m", "5maj7")
            string nashvilleChordPattern = @"^[1-7]";

            if (Regex.IsMatch(chord, letterChordPattern))
            {
                // Transpose letter notation chords
                return TransposeLetterChord(chord, steps);
            }
            else if (Regex.IsMatch(chord, nashvilleChordPattern))
            {
                // Transpose Nashville notation chords
                return TransposeNashville(chord, steps);
            }
            else
            {
                throw new ArgumentException($"Invalid chord format: {chord}");
            }
        }

        private static string TransposeLetterChord(string chord, int steps)
        {
            // Extract the root note (e.g., G# from G#m7) and the rest of the chord (e.g., m7 from G#m7)
            string pattern = @"^[A-G][#b]?";
            var match = Regex.Match(chord, pattern);

            if (!match.Success)
            {
                throw new ArgumentException($"Invalid chord: {chord}");
            }

            string rootNote = match.Value;  // Extract the root note (e.g., "G#", "F", etc.)
            string chordSuffix = chord[rootNote.Length..];  // Extract the suffix (e.g., "m7", "maj7", etc.)

            // Find the index of the root note in the chromatic scale
            int rootIndex = chromaticScale.IndexOf(rootNote);

            if (rootIndex == -1)
            {
                throw new ArgumentException($"Invalid root note: {rootNote}");
            }

            // Calculate the new root index after transposition
            int newRootIndex = (rootIndex + steps + chromaticScale.Count) % chromaticScale.Count;

            // Get the transposed root note
            string transposedRootNote = chromaticScale[newRootIndex];

            // Return the transposed chord with the original suffix
            return transposedRootNote + chordSuffix;
        }


        public static string TransposeNashville(string nashvilleChord, int steps)
        {
            // Pattern to match Nashville numbers (e.g., 1, 4, 5) with optional chord suffixes (e.g., "m", "7", etc.)
            string nashvillePattern = @"^([1-7])(m|maj|sus|dim|aug|7|add[0-9]*)?$";
            var match = Regex.Match(nashvilleChord, nashvillePattern);

            if (!match.Success)
            {
                throw new ArgumentException($"Invalid Nashville chord: {nashvilleChord}");
            }

            string chordNumber = match.Groups[1].Value;  // Extract the chord number (e.g., "1", "4", etc.)
            string chordSuffix = match.Groups[2].Value;  // Extract the suffix (e.g., "m", "7", etc.), if any

            // Find the index of the chord number in the Nashville scale
            int chordIndex = nashvilleScale.IndexOf(chordNumber);

            if (chordIndex == -1)
            {
                throw new ArgumentException($"Invalid chord number: {chordNumber}");
            }

            // Transpose the chord by adding the number of steps
            int transposedIndex = (chordIndex + steps + nashvilleScale.Count) % nashvilleScale.Count;

            // Get the transposed chord number
            string transposedChord = nashvilleScale[transposedIndex];

            // Return the transposed chord with the original suffix (if any)
            return transposedChord + chordSuffix;
        }

    }
}
