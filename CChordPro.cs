using DocumentFormat.OpenXml.Drawing;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static ChordSheetConverter.CAllConverters;
using static ChordSheetConverter.CChordSheetLine;

namespace ChordSheetConverter
{
    public class CChordPro : CBasicConverter, IChordSheetAnalyzer
    {

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

        public Dictionary<string, string> propertyMapTags { get; } = chordProMapTags;

        private (string tagName, string tagValue) getTags(string line)
        {
            if (line.StartsWith("{") && line.EndsWith('}'))
            {
                line = line.Trim('{', '}');
                var parts = line.Split(':');
                string tagName = parts[0].Trim();
                string tagValue = "";

                if (parts.Length > 1)
                {
                    tagValue = parts[1].Trim();
                    if (tagValue.Contains("label="))
                    {
                        int labelIndex = tagValue.IndexOf("label=") + 6;
                        tagValue = tagValue.Substring(labelIndex).Trim('"');
                    }
                }

                return (tagName, tagValue);
            }

            return ("", "");
        }

        public string getFirstValueFromSecondIn_propertyMapTags(string secondValue)
        {
            foreach (var kvp in propertyMapTags)
            {
                if (kvp.Value == secondValue)
                {
                    return kvp.Key; // Return the first value (property name)
                }
            }
            return ""; // Return null if no match is found
        }

        // Method to return the tags with their content as a formatted string, only for non-empty properties
        public string getChordProTags()
        {
            // Create a StringBuilder to accumulate the tag strings
            StringBuilder tags = new();

            // Get the properties listed in propertyMapTags (first value of the dictionary)
            var propertiesToExtract = propertyMapTags.Select(kvp => kvp.Key).ToHashSet();

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

        public List<CChordSheetLine> analyze(string text)
        {
            return analyze(stringToLines(text));
        }

        public List<CChordSheetLine> analyze(string[] lines)
        {
            List<CChordSheetLine> chordSheetLines = [];
            string lastSectionStart = "";

            // Process each line
            foreach (string line in lines)
            {
                if (line.Contains('{') && line.Contains(':') && line.Contains('}'))
                {
                    (string tagName, string tagValue) = getTags(line);
                    string propertyName = getFirstValueFromSecondIn_propertyMapTags(tagName);
                    if (propertyName != "")
                    {
                        // It is a property
                        setPropertyByName(propertyName, tagValue);
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
                    string chordLine = extractChords(line);   // Get the chord line
                    string textLine = removeChords(line);     // Get the corresponding text line

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

        // Helper method to extract chords from a line
        private static string extractChords(string line)
        {
            var result = new char[line.Length]; // Create a character array the size of the input line
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = ' '; // Initialize the array with spaces
            }

            // Match all chords in the format [C], [G], etc.
            var matches = Regex.Matches(line, @"\[([A-G][#b]?m?\d*)\]");

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
        // Helper method to remove chords from a line
        private static string removeChords(string line)
        {
            return Regex.Replace(line, @"\[([A-G][#b]?m?\d*)\]", "").Trim();
        }

        public override string build(List<CChordSheetLine> chordSheetLines)
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
            ns.Add(getChordProTags());

            int idx = 0;
            while (idx < CBasicConverter.chordSheetLines.Count)
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

                    line = Regex.Replace(line, @"\s+", " "); // Use Regex to replace multiple spaces with a single space
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
                        addMakeCommentString(ref ns, line);
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
                    addMakeCommentString(ref ns, line);
                }
                idx++;

            }
            return linesToString([.. ns]);

        }

        private static void addMakeCommentString(ref List<string> destination, string comment)
        {
            if (comment != "")
            {
                destination.Add("{comment:" + comment + "}");
            }
        }
    }
}
