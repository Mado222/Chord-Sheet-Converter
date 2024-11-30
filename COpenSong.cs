using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using static ChordSheetConverter.CScales;
using EnLineType = ChordSheetConverter.CChordSheetLine.EnLineType;

namespace ChordSheetConverter
{
    public partial class COpenSong : CBasicConverter
    {
        private static readonly Dictionary<string, string> ChordProSectionTags = new()
{
    { "V", "Verse" },
    { "B", "Bridge" },
    { "C", "Chorus" }
};

        // property / tag
        public static readonly Dictionary<string, string> openSongMapTags = new()
        {
    { "Title", "title" },
    { "Composer", "author" },
    { "Copyright", "copyright" },
    { "Key", "key" },
    { "Time", "time_sig" },
    { "Tempo", "tempo" },
    { "Capo", "capo" } };

        public override Dictionary<string, string> PropertyMapTags { get; } = openSongMapTags;

        private List<string> GetOpenSongTags()
        {
            var xmlElements = new List<string>();

            foreach (var property in PropertyMapTags)
            {
                var propInfo = this.GetType().GetProperty(property.Key);

                if (propInfo != null)
                {
                    var value = propInfo.GetValue(this, null) as string;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        // Create XML element and add to list
                        xmlElements.Add($"<{property.Value}>{value}</{property.Value}>");
                    }
                }
            }

            return xmlElements;
        }

        public override List<CChordSheetLine> Analyze(string[] lines) //returns chordSheetLines
        {
            return Analyze(LinesToString(lines));
        }

        [GeneratedRegex(@"^\[(\w)(\d*)\]")]
        private static partial Regex RegexAnalyze();

        public override List<CChordSheetLine> Analyze(string text) // Returns chordSheetLines
        {
            // Method to check the section tag and return the corresponding name
            static string checkSectionTag(string line)
            {
                // Use a regex to match patterns like [V], [V1], [C], [C2], etc.
                var match = RegexAnalyze().Match(line);

                if (match.Success)
                {
                    // Extract the tag (e.g., V, C, B) and optional number (e.g., 1)
                    string tag = match.Groups[1].Value;     // The letter part, e.g., V, B, C
                    string number = match.Groups[2].Value;  // The number part, if any

                    // Check if the tag exists in the dictionary
                    if (ChordProSectionTags.TryGetValue(tag, out string? value))
                    {
                        // Get the section name from the dictionary (e.g., Verse, Bridge, Chorus)
                        string sectionName = value;

                        // Return the section name, adding the number if it exists
                        return !string.IsNullOrEmpty(number) ? $"{sectionName} {number}" : sectionName;
                    }
                }

                // If no valid tag is found, return null or an empty string
                return "";
            }

            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), "Input text cannot be null.");
            }

            // DeEscape
            text = WebUtility.HtmlDecode(text);
            Dictionary<string, string> xmlContent = GetXmlElementContent(text);
            Type currentType = GetType();

            // Reverse mapping: Map XML tag (second value) to property name (first value)
            var reversedMapTags = openSongMapTags.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

            // XML content into properties, using the reversed map
            foreach (var kvp in xmlContent)
            {
                string xmlTag = kvp.Key;  // XML tag, e.g., "title", "composer", etc.
                string xmlValue = kvp.Value;  // The value of the XML element

                // Check if the XML tag is mapped to a property
                if (reversedMapTags.TryGetValue(xmlTag, out string propertyName))
                {
                    // Get the property by the name that matches the mapped property name
                    PropertyInfo property = currentType.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    // If the property exists and is writable, set its value
                    if (property != null && property.CanWrite)
                    {
                        // Set the value (assuming all properties are strings)
                        property.SetValue(this, xmlValue);
                    }
                }
            }

            // Process lyrics and other lines
            List<string> lyricsLines = new(StringToLines(xmlContent["lyrics"]));
            ChordSheetLines.Clear();
            EnLineType ltypeText = EnLineType.TextLineVerse;
            EnLineType ltypeChorus = EnLineType.ChordLineVerse;


            foreach (string line in lyricsLines)
            {
                if (line == "")
                {
                    ChordSheetLines.Add(new CChordSheetLine(EnLineType.EmptyLine, ""));
                }
                else
                {
                    char firstChar = line[0];
                    if (firstChar == '.')
                    {
                        ChordSheetLines.Add(new CChordSheetLine(ltypeChorus, line[1..]));
                    }
                    else if (firstChar == ' ' || firstChar == CChordSheetLine.nonBreakingSpaceChar)
                    {
                        ChordSheetLines.Add(new CChordSheetLine(ltypeText, line[1..]));
                    }
                    else if (firstChar == '[')
                    {
                        ChordSheetLines.Add(new CChordSheetLine(EnLineType.SectionBegin, checkSectionTag(line)));
                        if (line.Contains('C'))
                        {
                            ltypeText = EnLineType.TextLineChorus;
                            ltypeChorus = EnLineType.ChordLineChorus;
                        }
                        else if (line.Contains('B'))
                        {
                            ltypeText = EnLineType.TextLineBridge;
                            ltypeChorus = EnLineType.ChordLineBridge;
                        }
                        else //if (line.Contains('V'))
                        {
                            ltypeText = EnLineType.TextLineVerse;
                            ltypeChorus = EnLineType.ChordLineVerse;
                        }

                    }
                    else if (firstChar == ';')
                    {
                        ChordSheetLines.Add(new CChordSheetLine(EnLineType.CommentLine, line[1..]));
                    }
                    else if (line.StartsWith("---"))
                    {
                        ChordSheetLines.Add(new CChordSheetLine(EnLineType.ColumnBreak, ""));
                    }
                    else if (line.StartsWith("-!!"))
                    {
                        ChordSheetLines.Add(new CChordSheetLine(EnLineType.PageBreak, ""));
                    }
                }
            }

            // Remove "lyrics" from the XML content
            xmlContent.Remove("lyrics");

            // Clean up separators
            for (int i = 0; i < ChordSheetLines.Count; i++)
            {
                foreach (string s in CChordSheetLine.line_separators)
                    ChordSheetLines[i].Line = ChordSheetLines[i].Line.Replace(s, "");
            }

            return ChordSheetLines;
        }


        public override string Build(List<CChordSheetLine> chordSheetLines)
        {
            List<string> res = [];

            res.Clear();
            res.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            res.Add("<song>");
            res.Add($"<title>{Title}</title>");
            res.Add("<lyrics>");

            for (int i = 0; i < chordSheetLines.Count; i++)
            {
                string line = chordSheetLines[i].Line;
                EnLineType thisLineType = chordSheetLines[i].LineType;

                switch (thisLineType)
                {
                    case EnLineType.ChordLineVerse:
                    case EnLineType.ChordLineBridge:
                    case EnLineType.ChordLineChorus:
                        //chord line
                        res.Add("." + line.TrimEnd());
                        break;
                    case EnLineType.EmptyLine:
                        //Empty line
                        res.Add("");
                        break;
                    case EnLineType.TextLine:
                    case EnLineType.TextLineVerse:
                    case EnLineType.TextLineChorus:
                    case EnLineType.TextLineBridge:
                        res.Add(' ' + line.Trim());
                        break;
                    case EnLineType.CommentLine:
                        //Any other textline
                        res.Add(";" + line.Trim());
                        break;
                    case EnLineType.SectionBegin:
                        {
                            if (line.Contains("Ver"))
                            {
                                //Get number from string
                                string vnum = new(line.Where(char.IsDigit).ToArray());
                                res.Add("[V" + vnum + "]");
                            }
                            else if (line.Contains("Chor"))
                            {
                                res.Add("[C]");
                            }
                            else if (line.Contains("Bri"))
                            {
                                res.Add("[B]");
                            }

                            break;
                        }

                    case EnLineType.SectionEnd:
                        if (line != "")
                            res.Add("; End " + line);
                        break;
                    case EnLineType.PageBreak:
                        res.Add("-!!");
                        break;
                    case EnLineType.ColumnBreak:
                        res.Add("---");
                        break;
                }
            }

            //close xml tags
            res.Add("</lyrics >");
            
            foreach (string s in GetOpenSongTags())
            {
                if (!s.Contains("<title>"))
                    res.Add(s);
            }
            res.Add("</song >");

            return LinesToString ([..res]);
        }

        public override string[] Transpose(string[] linesIn, TranspositionParameters? parameters = null, int? steps = null )
        {
            var transposedLines = new List<string>();

            foreach (var lineIn in linesIn)
            {
                if (lineIn.StartsWith('.'))
                {
                    // Extract chords from the incoming line
                    (CChordCollection chords, string _) = ExtractChords(lineIn[1..]);

                    // Transpose each chord using CScales class
                    foreach (var chord in chords)
                    {
                        if (steps != null)
                        {
                            chord.Chord = CScales.Transpose(chord.Chord, (int) steps);
                        }
                        else if (parameters != null)
                        {
                            chord.Chord = CScales.Transpose(chord.Chord, parameters);
                        }
                    }

                    // Retrieve the transposed, well-spaced chord line
                    string transposedLine = "." + chords.GetWellSpacedChordLine();
                    transposedLines.Add(transposedLine);
                }
                else
                {
                    transposedLines.Add(lineIn);
                }
            }
            return [.. transposedLines];
        }

        // Helper method to extract chords from a line
        public override (CChordCollection chords, string lyrics) ExtractChords(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return (new CChordCollection(), "");

            if (line.StartsWith(".")) line = line[1..];
            if (string.IsNullOrEmpty(line)) return (new CChordCollection(), "");

            var chords = new CChordCollection();
            var chordBuilder = new StringBuilder();
            int idxChordBegin = -1;

            line = line.TrimEnd() + ' '; // Ensure the last chord is processed

            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] != ' ' && line[i] != '\u00A0')
                {
                    if (idxChordBegin == -1) idxChordBegin = i; // Mark the start of a chord
                    chordBuilder.Append(line[i]);
                }
                else if (chordBuilder.Length > 0)
                {
                    // End of a chord, add to the collection
                    if (!CScales.IsValidChord(chordBuilder.ToString()))
                        return (new CChordCollection(), "");
                    chords.AddChord(new CChord(chordBuilder.ToString(), idxChordBegin));
                    chordBuilder.Clear();

                    idxChordBegin = -1;
                }
            }

            return (chords, "");
        }

        public override string ConverToNashville(string text, string key, ScaleType scaleType = ScaleType.Major)
        {
            List<CChordSheetLine> linesOut = [];

            List<CChordSheetLine> lines = Analyze(text);
            foreach (CChordSheetLine line in lines)
            {
                if (line.LineType == EnLineType.ChordLineChorus || line.LineType == EnLineType.ChordLineBridge || line.LineType == EnLineType.ChordLineVerse)
                {
                    CChordCollection chordsOut = new();
                    (CChordCollection chords, _) = ExtractChords(line.Line);
                    foreach (CChord c in chords)
                    {
                        c.Chord = ConvertChordToNashville(c.Chord, key, scaleType);
                        chordsOut.AddChord(c);
                    }
                    line.Line = chordsOut.GetWellSpacedChordLine();
                    linesOut.Add(line);
                }
                else
                {
                    linesOut.Add(line);
                }
            }
            return Build(linesOut);
        }
    }
}
