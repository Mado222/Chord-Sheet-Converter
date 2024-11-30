using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ScaleType = ChordSheetConverter.CScales.ScaleType;

namespace ChordSheetConverter
{
    public class CTranspose
    {
        private static readonly Dictionary<ScaleType, string[]> HarmonizedChords = new Dictionary<ScaleType, string[]>()
    {
        { ScaleType.Major, new[] { "maj", "min", "min", "maj", "maj", "min", "dim" } },
        { ScaleType.NaturalMinor, new[] { "min", "dim", "maj", "min", "min", "maj", "maj" } },
        { ScaleType.HarmonicMinor, new[] { "min", "dim", "aug", "min", "maj", "maj", "dim" } },
        { ScaleType.MelodicMinor, new[] { "min", "min", "aug", "maj", "maj", "dim", "dim" } }
    };

        public static string TransposeChord(string chord, TranspositionParameters parameters)
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

            // Determine the scale degree of the chord in the source scale
            int scaleDegree = DetermineScaleDegree(rootNote, parameters.SourceKey, parameters.SourceScaleType);

            // Transpose the root note
            string transposedRootNote = TransposeRootNote(rootNote, parameters);

            // Get the corresponding chord type in the target scale
            string targetChordType = HarmonizedChords[parameters.TargetScaleType][scaleDegree - 1];

            // Return the transposed chord with the appropriate type
            return transposedRootNote + targetChordType;
        }

        private static int DetermineScaleDegree(string rootNote, string key, ScaleType scaleType)
        {
            // Assuming we have a method to determine the scale degree of a chord
            // This method will determine the number (1-7) of the chord within the source scale
            var scaleNotes = CScales.GetScaleNotes(key, scaleType);
            int index = scaleNotes.IndexOf(rootNote);
            if (index == -1)
            {
                throw new ArgumentException($"The root note {rootNote} is not part of the {scaleType} scale of {key}");
            }
            return index + 1;
        }

        private static string TransposeRootNote(string rootNote, TranspositionParameters parameters)
        {
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
            return CScales.chromaticScale[newRootIndex];
        }

        private static string NormalizeRootNoteToSharp(string rootNote)
        {
            // Convert flat notes to their sharp equivalents
            return rootNote.Replace("Bb", "A#").Replace("Db", "C#").Replace("Eb", "D#").Replace("Gb", "F#").Replace("Ab", "G#");
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
    }
}
