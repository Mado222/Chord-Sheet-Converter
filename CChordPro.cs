using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static ChordSheetConverter.CChordSheetLine;
using static ChordSheetConverter.CScales;

namespace ChordSheetConverter
{
    public partial class CChordPro : CBasicConverter    {

        // property / tag
        public static readonly Dictionary<string, string> chordProMapTags = new()
        {
    { "title", "title" },
    { "subTitle", "subTitle" },
    { "composer", "composer" },
    { "lyricist", "lyricist" },
    { "copyright", "copyright" },
    { "album", "album" },
    { "year", "year" },
    { "key", "key" },
    { "time", "time" },
    { "tempo", "tempo" },
    { "duration", "duration" },
    { "capo", "capo" }
};

        public override Dictionary<string, string> PropertyMapTags { get; } = chordProMapTags;

        public string GetFirstValueFromSecondIn_propertyMapTags(string secondValue)
        {
            foreach (var kvp in PropertyMapTags)
            {
                if (kvp.Value == secondValue)
                {
                    return kvp.Key; // Return the first value (property name)
                }
            }
            return ""; // Return null if no match is found
        }

        // Method to return the tags with their content as a formatted string, only for non-empty properties
        public string GetChordProTags()
        {
            // Create a StringBuilder to accumulate the tag strings
            StringBuilder tags = new();

            // Get the properties listed in propertyMapTags (first value of the dictionary)
            var propertiesToExtract = PropertyMapTags.Select(kvp => kvp.Key).ToHashSet();

            // Get all the properties of this class
            PropertyInfo[] properties = this.GetType().GetProperties();

            foreach (var property in properties)
            {
                // Check if the property name exists in propertyMapTags
                if (propertiesToExtract.Contains(property.Name))
                {
                    // Get the value of the property
                    var value = property.GetValue(this)?.ToString();

                    // Only add the tag if the value is not null or empty
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Format and append the tag to the StringBuilder
                        tags.AppendLine($"{{{property.Name.ToLower()}: {value}}}");
                    }
                }
            }

            // Return the accumulated tags as a string
            return tags.ToString();
        }

        public override List<CChordSheetLine> Analyze(string text)
        {
            return Analyze(StringToLines(text));
        }

        public override List<CChordSheetLine> Analyze(string[] lines)
        {
            List<CChordSheetLine> chordSheetLines = [];
            string lastSectionStart = "";

            // Process each line
            foreach (string line in lines)
            {
                if (line.Contains('{') && line.Contains(':') && line.Contains('}'))
                {
                    (string tagName, string tagValue) = GetTags(line);
                    string propertyName = GetFirstValueFromSecondIn_propertyMapTags(tagName);
                    if (propertyName != "")
                    {
                        // It is a property
                        SetPropertyByName(propertyName, tagValue);
                        chordSheetLines.Add(new CChordSheetLine(enLineType.xmlElement, line));
                        continue;
                    }
                    //Any other tag
                    if (line.Contains("{start_of_", StringComparison.CurrentCultureIgnoreCase))
                    {
                        chordSheetLines.Add(new CChordSheetLine(enLineType.SectionBegin, tagValue));
                        lastSectionStart = tagValue;
                        continue;
                    }
                    if (line.Contains("{comment", StringComparison.CurrentCultureIgnoreCase))
                    {
                        chordSheetLines.Add(new CChordSheetLine(enLineType.CommentLine, tagValue));
                        continue;
                    }
                    else
                    {
                        chordSheetLines.Add(new CChordSheetLine(enLineType.xmlElement, line));
                        continue;
                    }
                }
                //Command
                if (line.Contains('{') && line.Contains('}'))
                {
                    if (line.Contains("{end_of_", StringComparison.CurrentCultureIgnoreCase) ||
                        line.Contains("{eo", StringComparison.CurrentCultureIgnoreCase))
                    {
                        chordSheetLines.Add(new CChordSheetLine(enLineType.SectionEnd, lastSectionStart));
                        lastSectionStart = "";
                        continue;
                    }
                }

                // Check if the line contains chords
                if (line.Contains('[') && line.Contains(']'))
                {
                    // Extract the chords and lyrics
                    string chordLine = ExtractChords(line);   // Get the chord line
                    string textLine = RemoveChords(line);     // Get the corresponding text line

                    // Add ChordLine and TextLine separately
                    chordSheetLines.Add(new CChordSheetLine(enLineType.ChordLine, chordLine));
                    if (!string.IsNullOrEmpty(textLine))
                        chordSheetLines.Add(new CChordSheetLine(enLineType.TextLine, textLine));
                    continue;
                }
                if (line == "")
                {
                    chordSheetLines.Add(new CChordSheetLine(enLineType.EmptyLine, ""));
                    continue;
                }
                chordSheetLines.Add(new CChordSheetLine(enLineType.Unknown, line));
            }
            return chordSheetLines;
        }

        [GeneratedRegex(@"\[([A-G][#b]?m?\d*)\]")]
        private static partial Regex RegexExtractChords();

        // Helper method to extract chords from a line
        private static string ExtractChords(string line)
        {
            var result = new char[line.Length]; // Create a character array the size of the input line
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ' '; // Initialize the array with spaces
            }

            // Match all chords in the format [C], [G], etc.
            var matches = RegexExtractChords().Matches(line);

            foreach (Match match in matches)
            {
                string chord = match.Groups[1].Value; // Extract the chord name
                int index = match.Index; // Get the position of the chord in the original string

                // Replace the space in the result array with the chord at the exact position
                for (int i = 0; i < chord.Length; i++)
                {
                    result[index + 1 + i] = chord[i]; // +1 to skip the opening '['
                }
            }

            return new string(result); // Convert the character array back to a string
        }

        [GeneratedRegex(@"\[([A-G][#b]?m?\d*)\]")]
        private static partial Regex RegexRemoveChords();

        // Helper method to remove chords from a line
        private static string RemoveChords(string line)
        {
            return RegexRemoveChords().Replace(line, "").Trim();
        }

        [GeneratedRegex(@"\s+")]
        private static partial Regex RegexBuild();

        public override string Build(List<CChordSheetLine> chordSheetLines)
        {
            List<string> ns = []; //new song
            bool inChorus = false;
            bool inVerse = false;
            bool inBridge = false;

            void endsections()
            {
                if (inChorus)
                {
                    //Close Chorus section
                    ns.Add("{eoc}");
                    inChorus = false;
                }
                if (inVerse)
                {
                    ns.Add("{eov}");
                    inVerse = false;
                }
                if (inBridge)
                {
                    ns.Add("{eob}");
                    inVerse = false;
                }
            }


            ns.Add("{ns}");
            ns.Add(GetChordProTags());

            int idx = 0;
            while (idx < CBasicConverter.ChordSheetLines.Count)
            {
                enLineType thisLineType = chordSheetLines[idx].lineType;

                if (idx < chordSheetLines.Count - 1)
                {
                    if ((thisLineType == enLineType.ChordLine) &&
                        chordSheetLines[idx + 1].lineType == enLineType.TextLine)
                    {
                        //Combine lines
                        //It is a chord line + lyrics combination
                        string chords = chordSheetLines[idx].line;

                        string lyric = chordSheetLines[idx + 1].line.Trim();
                        string chord = "";
                        int offset = 0;
                        for (int i = 0; i < chords.Length; i++)
                        {
                            if (chords[i] != ' ')
                            {
                                chord += chords[i];
                            }
                            if (chords[i] == ' ' || i == chords.Length - 1)
                            {
                                if (chord != "" && chord != " " && chord != CChordSheetLine.nonBreakingSpace)
                                {
                                    int j = i - chord.Length;
                                    if (j < 0) j = 0;       //insert chord @ beginning of the line
                                    if (i - chord.Length + offset < lyric.Length)
                                        lyric = lyric[..(j + offset)] + "[" + chord.Trim() + "]" + lyric[(j + offset)..];
                                    else
                                        lyric += chord;
                                    offset += chord.Length + 2;
                                    chord = "";
                                }
                            }
                        }

                        ns.Add(lyric);
                        idx += 2;
                        continue;
                    }
                }


                //Isolated Line
                string line = chordSheetLines[idx].line.Trim();
                if (thisLineType == enLineType.ChordLine)
                {
                    //Chorus without following text line = isolated chord line

                    line = RegexBuild().Replace(line, " "); // Use Regex to replace multiple spaces with a single space
                    string[] ss = line.Split(' ');
                    string allchords = "";
                    for (int k = 0; k < ss.Length; k++)
                    {
                        if (ss[k] != " " && ss[k] != CChordSheetLine.nonBreakingSpace)
                        {
                            //Surround Chords with []
                            allchords += '[' + ss[k] + "] ";
                        }
                    }
                    ns.Add(allchords);
                }
                else if (thisLineType == enLineType.SectionBegin)
                {
                    if (line.Contains('V'))
                    {
                        endsections();
                        ns.Add($"{{start_of_verse: label=\"{line}\"}}");
                        inVerse = true;
                    }
                    if (line.Contains('C'))
                    {
                        endsections();
                        ns.Add($"{{start_of_chorus: label=\"{line}\"}}");
                        inChorus = true;
                    }

                    if (line.Contains('B'))
                    {
                        endsections();
                        ns.Add($"{{start_of_bridge: label=\"{line}\"}}");
                        inBridge = true;
                    }
                }
                else if (thisLineType == enLineType.CommentLine)
                {
                    //Comment
                    if (line.Length > 0)
                    {
                        AddMakeCommentString(ref ns, line);
                    }
                }
                else if (thisLineType == enLineType.ColumnBreak)
                {
                    //New Column
                    ns.Add("{column_break}");
                }
                else if (thisLineType == enLineType.PageBreak)
                {
                    //New Page
                    ns.Add("{new_page}");
                }
                else if (thisLineType == enLineType.EmptyLine)
                {
                    //Line break = empty line
                    endsections();
                    ns.Add("");
                }
                else
                {
                    AddMakeCommentString(ref ns, line);
                }
                idx++;

            }
            return LinesToString([.. ns]);

        }

        private static void AddMakeCommentString(ref List<string> destination, string comment)
        {
            if (comment != "")
            {
                destination.Add("{comment:" + comment + "}");
            }
        }

        //Transpose Letter or Nashville
        public static string TransposeChordPro(string textIn, int steps, string key="", ScaleType scaleType = ScaleType.Major)
        {
            return LinesToString(TransposeChordPro(StringToLines(textIn), steps, key, scaleType));
        }

        //Transpose Letter or Nashville
        public static string[] TransposeChordPro(string[] linesIn, int steps, string key="", ScaleType scaleType = ScaleType.Major)
        {
            List<string> transposedLines = [];

            foreach (string line in linesIn)
            {
                if (line.Contains('[') && line.Contains(']'))
                {
                    if (!string.IsNullOrEmpty(key))
                        transposedLines.Add(TransposeChordProLineNashville(line, steps, key, scaleType));
                    else
                        transposedLines.Add(TransposeChordProLine(line, steps));
                }
                else
                {
                    transposedLines.Add(line);  // No chords in the line, add it unchanged
                }
            }

            return [.. transposedLines];
        }

        //One letter line
        private static string TransposeChordProLine(string lineIn, int steps)
        {
            // Pattern to match chords within square brackets, e.g. [C], [F#], [G#m]
            string chordPattern = @"\[([A-G][#b]?m?(maj|sus|dim|aug)?[0-9]?(add[0-9])?)\]";

            return Regex.Replace(lineIn, chordPattern, match =>
            {
                // Extract the chord inside the brackets (without the brackets)
                string chord = match.Groups[1].Value;

                // Transpose the chord
                string transposedChord = CScales.Transpose(chord, steps);

                // Return the transposed chord wrapped in brackets
                return $"[{transposedChord}]";
            });
        }

        //One Nashville line
        private static string TransposeChordProLineNashville(string line, int steps, string key, ScaleType scaleType)
        {
            // Pattern to match chords within square brackets, e.g., [1], [4m], [5maj7]
            string chordPattern = @"\[([1-7](m|maj|sus|dim|aug|7|add[0-9]*)?)\]";

            // Replace each Nashville chord in the line
            return Regex.Replace(line, chordPattern, match =>
            {
                // Extract the Nashville chord (without brackets)
                string nashvilleChord = match.Groups[1].Value;

                // Transpose the Nashville chord
                string transposedChord = CScales.TransposeNashville(nashvilleChord, steps);

                // Return the transposed Nashville chord wrapped in brackets
                return $"[{transposedChord}]";
            });
        }


        public static string ConvertChordProToNashville(string textIn, string key, ScaleType scaleType = ScaleType.Major)
        {
            return LinesToString(ConvertChordProToNashville(StringToLines(textIn), key,scaleType));
        }
        public static string[] ConvertChordProToNashville(string[] linesIn, string key, ScaleType scaleType = ScaleType.Major)
        {
            List<string> nashvilleLines = [];

            foreach (string line in linesIn)
            {
                if (line.Contains('[') && line.Contains(']'))
                {
                    nashvilleLines.Add(ConvertChordProLineToNashville(line, key, scaleType));
                }
                else
                {
                    nashvilleLines.Add(line);  // No chords in the line, add it unchanged
                }
            }

            return [.. nashvilleLines];
        }

        private static string ConvertChordProLineToNashville(string line, string key, ScaleType scaleType)
        {
            // Pattern to match letter chords within square brackets, e.g., [C], [G#m7]
            string chordPattern = @"\[([A-G][#b]?m?(maj|sus|dim|aug)?[0-9]?(add[0-9])?)\]";

            // Replace each letter chord in the line
            return Regex.Replace(line, chordPattern, match =>
            {
                // Extract the letter chord (without brackets)
                string letterChord = match.Groups[1].Value;

                // Convert the letter chord to Nashville notation
                string nashvilleChord = CScales.ConvertChordToNashville(letterChord, key, scaleType);

                // Return the Nashville chord wrapped in brackets
                return $"[{nashvilleChord}]";
            });
        }
    }
}

