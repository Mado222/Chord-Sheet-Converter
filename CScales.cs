using System.ComponentModel;
using System.Text.RegularExpressions;
using static ChordSheetConverter.CScales;

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
        }

        public enum ScaleMode
        {
            Ionian,       // Major
            Dorian,
            Phrygian,
            Lydian,
            Mixolydian
        }

        public enum ScaleTypeAll
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
        };

        private static readonly Dictionary<ScaleTypeAll, List<string>> scaleIntervalsAll = new()
{
    // Major and Minor scales
    { ScaleTypeAll.Major, new List<string> { "C", "D", "E", "F", "G", "A", "B" } },
    { ScaleTypeAll.NaturalMinor, new List<string> { "A", "B", "C", "D", "E", "F", "G" } },
    { ScaleTypeAll.HarmonicMinor, new List<string> { "A", "B", "C", "D", "E", "F", "G#" } },
    { ScaleTypeAll.MelodicMinor, new List<string> { "A", "B", "C", "D", "E", "F#", "G#" } },

    // Modes derived from the Major scale
    { ScaleTypeAll.Ionian, new List<string> { "C", "D", "E", "F", "G", "A", "B" } },          // Same as Major
    { ScaleTypeAll.Dorian, new List<string> { "D", "E", "F", "G", "A", "B", "C" } },
    { ScaleTypeAll.Phrygian, new List<string> { "E", "F", "G", "A", "B", "C", "D" } },
    { ScaleTypeAll.Lydian, new List<string> { "F", "G", "A", "B", "C", "D", "E" } },
    { ScaleTypeAll.Mixolydian, new List<string> { "G", "A", "B", "C", "D", "E", "F" } },
    { ScaleTypeAll.Aeolian, new List<string> { "A", "B", "C", "D", "E", "F", "G" } },         // Same as Natural Minor
    { ScaleTypeAll.Locrian, new List<string> { "B", "C", "D", "E", "F", "G", "A" } },

    // Pentatonic scales
    { ScaleTypeAll.MajorPentatonic, new List<string> { "C", "D", "E", "G", "A" } },
    { ScaleTypeAll.MinorPentatonic, new List<string> { "A", "C", "D", "E", "G" } },

    // Other common scales
    { ScaleTypeAll.BluesScale, new List<string> { "A", "C", "D", "D#", "E", "G" } },
    { ScaleTypeAll.ChromaticScale, new List<string> { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" } },
    { ScaleTypeAll.WholeToneScale, new List<string> { "C", "D", "E", "F#", "G#", "A#" } },
    { ScaleTypeAll.HalfWholeDiminished, new List<string> { "C", "D♭", "E♭", "E", "F#", "G", "A", "B♭" } },
    { ScaleTypeAll.WholeHalfDiminished, new List<string> { "C", "D", "E♭", "F", "F#", "G#", "A", "B" } },
    { ScaleTypeAll.AugmentedScale, new List<string> { "C", "E", "G#", "B", "C#" } },

    // Exotic scales
    { ScaleTypeAll.SpanishPhrygian, new List<string> { "C", "D♭", "E", "F", "G", "A♭", "B♭" } },
    { ScaleTypeAll.HungarianMinor, new List<string> { "C", "D", "E♭", "F#", "G", "A♭", "B" } },
    { ScaleTypeAll.JapaneseScale, new List<string> { "C", "D", "E♭", "G", "A♭" } }
};

        public static List<string> GetScaleNotes(string rootNote, ScaleType scaleType)
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


        // Function to get the notes of a scale given its ScaleType and root note
        public static List<string> GetScaleNotes(ScaleTypeAll scaleType, string rootNote)
        {
            // Get the scale degrees from the dictionary
            if (!scaleIntervalsAll.TryGetValue(scaleType, out List<string>? value))
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

        public static string Transpose(string chord, TranspositionParameters parameters)
        {
            // Pattern to detect letter notation chords (e.g., "G#m7", "F#maj7", "Cadd9")
            string letterChordPattern = @"^[A-G][#b]?";

            // Pattern to detect Nashville notation (e.g., "1", "4m", "5maj7")
            string nashvilleChordPattern = @"^[1-7]";

            if (Regex.IsMatch(chord, letterChordPattern))
            {
                // Transpose letter notation chords
                return TransposeLetterChord(chord, parameters);
            }
            else if (Regex.IsMatch(chord, nashvilleChordPattern))
            {
                // Transpose Nashville notation chords
                //return TransposeNashville(chord, sourceKey, sourceScaleType, targetKey, targetScaleType);
                return "";
            }
            else
            {
                throw new ArgumentException($"Invalid chord format: {chord}");
            }
        }
        private static string TransposeLetterChord(string chord, TranspositionParameters parameters)
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

            // Normalize root note to its equivalent if it is flat (Bb -> A# etc.)
            rootNote = NormalizeRootNoteToSharp(rootNote);

            // Find the index of the root note in the chromatic scale
            int rootIndex = CScales.chromaticScale.IndexOf(rootNote);

            if (rootIndex == -1)
            {
                throw new ArgumentException($"Invalid root note: {rootNote}");
            }

            // Calculate the steps needed for transposition based on source and target keys
            int steps = CalculateStepsBetweenKeys(parameters);
            int newRootIndex = (rootIndex + steps + CScales.chromaticScale.Count) % CScales.chromaticScale.Count;

            // Get the transposed root note
            string transposedRootNote = CScales.chromaticScale[newRootIndex];

            // Adjust the representation based on the target key and scale type
            transposedRootNote = AdjustEnharmonicRepresentation(transposedRootNote, parameters.TargetKey, parameters.TargetScaleType);

            // Update the chord suffix based on the target scale type if necessary
            chordSuffix = AdjustChordSuffixForScaleType(chordSuffix, parameters);

            // Return the transposed chord with the updated suffix
            return transposedRootNote + chordSuffix;
        }

        private static string NormalizeRootNoteToSharp(string rootNote)
        {
            // Convert flat notes to their sharp equivalents
            return rootNote.Replace("Bb", "A#").Replace("Db", "C#").Replace("Eb", "D#").Replace("Gb", "F#").Replace("Ab", "G#");
        }

        private static string AdjustEnharmonicRepresentation(string note, string key, ScaleType scaleType)
        {
            // Determine if the destination key is major or minor
            bool isMinorKey = scaleType == ScaleType.NaturalMinor || scaleType == ScaleType.HarmonicMinor || scaleType == ScaleType.MelodicMinor || key.EndsWith("m");

            // List of keys that prefer flats
            var flatKeys = new HashSet<string> { "F", "Bb", "Eb", "Ab", "Db", "Gb", "Cb", "Dm", "Gm", "Cm", "Fm", "Bbm", "Ebm", "Abm" };

            // If the key is in the list of flat keys, prefer flat representation
            if (flatKeys.Contains(key) || isMinorKey)
            {
                return note.Replace("A#", "Bb").Replace("C#", "Db").Replace("D#", "Eb").Replace("F#", "Gb").Replace("G#", "Ab");
            }
            else
            {
                // Otherwise, prefer sharp representation
                return note.Replace("Bb", "A#").Replace("Db", "C#").Replace("Eb", "D#").Replace("Gb", "F#").Replace("Ab", "G#");
            }
        }

        private static string AdjustChordSuffixForScaleType(string chordSuffix, TranspositionParameters parameters)
        {
            // Adjust the chord suffix based on the target key and scale type for harmonic minor
            if (parameters.TargetScaleType == ScaleType.HarmonicMinor)
            {
                switch (chordSuffix)
                {
                    case "m":
                        return "m"; // i chord is minor
                    case "":
                        return parameters.TargetKey == "G" ? "" : "m"; // V chord is major, others are minor
                    case "dim":
                        return "dim"; // ii° chord is diminished
                    default:
                        return chordSuffix;
                }
            }

            // Adjust chord suffix for natural minor
            if (parameters.TargetScaleType == ScaleType.NaturalMinor)
            {
                switch (chordSuffix)
                {
                    case "":
                        if (parameters.SourceScaleType == ScaleType.Major)
                        {
                            return "m"; // I chord becomes minor in natural minor
                        }
                        break;
                    case "m":
                        return "dim"; // ii chord becomes diminished in natural minor
                    default:
                        return chordSuffix;
                }
            }

            // Adjust chord suffix for melodic minor
            if (parameters.TargetScaleType == ScaleType.MelodicMinor)
            {
                switch (chordSuffix)
                {
                    case "":
                        if (parameters.SourceScaleType == ScaleType.Major)
                        {
                            return "m"; // I chord becomes minor in melodic minor
                        }
                        break;
                    case "m":
                        return "dim"; // ii chord becomes diminished in melodic minor
                    default:
                        return chordSuffix;
                }
            }

            // Adjust chord suffix for modes
            if (parameters.TargetScaleType == ScaleType.Major)
            {
                switch (parameters.TargetMode)
                {
                    case ScaleMode.Dorian:
                        if (chordSuffix == "m") return "m"; // Dorian ii chord is minor
                        if (chordSuffix == "") return "7"; // Dorian IV chord is dominant 7
                        break;
                    case ScaleMode.Phrygian:
                        if (chordSuffix == "m") return "m"; // Phrygian i chord is minor
                        break;
                    case ScaleMode.Lydian:
                        if (chordSuffix == "") return ""; // Lydian I chord is major
                        break;
                    case ScaleMode.Mixolydian:
                        if (chordSuffix == "") return "7"; // Mixolydian I chord is dominant 7
                        break;
                }
            }

            // Default: no change to the chord suffix
            return chordSuffix;
        }

        private static int CalculateStepsBetweenKeys(TranspositionParameters parameters)
        {
            // Find the root index of the source and target keys in the chromatic scale
            int sourceIndex = CScales.chromaticScale.IndexOf(NormalizeRootNoteToSharp(parameters.SourceKey));
            int targetIndex = CScales.chromaticScale.IndexOf(NormalizeRootNoteToSharp(parameters.TargetKey));

            if (sourceIndex == -1 || targetIndex == -1)
            {
                throw new ArgumentException("Invalid source or target key");
            }

            // Calculate the number of steps between the source and target keys
            int steps = targetIndex - sourceIndex;

            // Ensure the steps value is within the range of -11 to 11 for optimal transposition
            if (steps < -6)
            {
                steps += 12;
            }
            else if (steps > 6)
            {
                steps -= 12;
            }

            return steps;
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
            string chordSuffix = chord.Substring(rootNote.Length);  // Extract the suffix (e.g., "m7", "maj7", etc.)

            // Find the index of the root note in the chromatic scale
            int rootIndex = CScales.chromaticScale.IndexOf(rootNote);

            if (rootIndex == -1)
            {
                throw new ArgumentException($"Invalid root note: {rootNote}");
            }

            // Calculate the new root index after transposition
            int newRootIndex = (rootIndex + steps + CScales.chromaticScale.Count) % CScales.chromaticScale.Count;

            // Get the transposed root note
            string transposedRootNote = CScales.chromaticScale[newRootIndex];

            // Check if the transposed root note is valid
            if (string.IsNullOrEmpty(transposedRootNote))
            {
                throw new ArgumentException($"Error in transposing chord: {chord}");
            }

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

        public static string GuessKey(string[] chords)
        {
            if (chords == null || chords.Length == 0) return "";


            // Extract the root note from a chord
            string ExtractRoot(string chord) => Regex.Match(chord, @"^[A-G][#b]?").Value;

            // Start by assuming the key is the root note of the last chord
            string lastChord = chords[^1];
            string assumedKey = ExtractRoot(lastChord);

            // Define common chord groups for I, IV, and V in a Major scale
            Dictionary<string, List<string>> majorChords = chromaticScale
                .ToDictionary(
                    root => root,
                    root => GetScaleNotes(root, ScaleType.Major)
                        .Where((note, i) => i == 0 || i == 3 || i == 4) // I, IV, V degrees
                        .ToList()
                );

            // Count how many chords align with each key
            Dictionary<string, int> keyScores = chromaticScale.ToDictionary(note => note, note => 0);
            foreach (string chord in chords)
            {
                string rootNote = ExtractRoot(chord);

                foreach (var key in chromaticScale)
                {
                    if (majorChords[key].Contains(rootNote))
                        keyScores[key]++;
                }
            }

            // Select the most likely key
            string mostLikelyKey = keyScores.OrderByDescending(kv => kv.Value).First().Key;

            // Return the most likely key, prioritizing the last chord's root if there's a tie
            return keyScores[mostLikelyKey] == keyScores[assumedKey] ? assumedKey : mostLikelyKey;
        }


    }
}
