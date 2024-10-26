using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using enLineType = ChordSheetConverter.CChordSheetLine.enLineType;

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
    { "title", "title" },
    { "composer", "author" },
    { "copyright", "copyright" },
    { "key", "key" },
    { "time", "time_sig" },
    { "tempo", "tempo" },
    { "capo", "capo" } };

        public override Dictionary<string, string> PropertyMapTags { get; } = openSongMapTags;

        public List<string> GetOpenSongTags()
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

            foreach (string line in lyricsLines)
            {
                if (line == "")
                {
                    ChordSheetLines.Add(new CChordSheetLine(enLineType.EmptyLine, ""));
                }
                else
                {
                    char firstChar = line[0];
                    if (firstChar == '.')
                    {
                        ChordSheetLines.Add(new CChordSheetLine(enLineType.ChordLine, line[1..]));
                    }
                    else if (firstChar == ' ' || firstChar == CChordSheetLine.nonBreakingSpaceChar)
                    {
                        ChordSheetLines.Add(new CChordSheetLine(enLineType.TextLine, line[1..]));
                    }
                    else if (firstChar == '[')
                    {
                        ChordSheetLines.Add(new CChordSheetLine(enLineType.SectionBegin, checkSectionTag(line)));
                    }
                    else if (firstChar == ';')
                    {
                        ChordSheetLines.Add(new CChordSheetLine(enLineType.CommentLine, line[1..]));
                    }
                    else if (line.StartsWith("---"))
                    {
                        ChordSheetLines.Add(new CChordSheetLine(enLineType.ColumnBreak, ""));
                    }
                    else if (line.StartsWith("-!!"))
                    {
                        ChordSheetLines.Add(new CChordSheetLine(enLineType.PageBreak, ""));
                    }
                }
            }

            // Remove "lyrics" from the XML content
            xmlContent.Remove("lyrics");

            // Clean up separators
            for (int i = 0; i < ChordSheetLines.Count; i++)
            {
                foreach (string s in CChordSheetLine.line_separators)
                    ChordSheetLines[i].line = ChordSheetLines[i].line.Replace(s, "");
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
                string line = chordSheetLines[i].line;
                enLineType thisLineType = chordSheetLines[i].lineType;

                if (thisLineType == enLineType.ChordLine)
                {
                    //chord line
                    res.Add("." + line.Trim());
                }
                else if (thisLineType == enLineType.EmptyLine)
                {
                    //Empty line
                    res.Add("");
                }
                else if (thisLineType == enLineType.TextLine)
                {
                    res.Add(' ' + line.Trim());
                }
                else if (thisLineType == enLineType.CommentLine)
                {
                    //Any other textline
                    res.Add(";" + line.Trim());
                }
                else if (thisLineType == enLineType.SectionBegin)
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
                }
                else if (thisLineType == enLineType.SectionEnd)
                {
                    if (line != "")
                        res.Add("; End " + line);
                }
                else if (thisLineType == enLineType.PageBreak)
                {
                    res.Add("-!!");
                }
                else if (thisLineType == enLineType.ColumnBreak)
                {
                    res.Add("---");
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

    }
}
