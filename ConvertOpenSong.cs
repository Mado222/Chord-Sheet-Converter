using System.Net;
using System.Text;
using System.IO;
using DocumentFormat.OpenXml.Bibliography;
using System.Text.RegularExpressions;

namespace ChordSheetConverter
{
    public class ConvertOpenSong : CBasicConverter
    {
        /*private static Dictionary<string, string> ChordProGeneralTags = new ()
{
    { "---", "ColumnBreak" },
    { ";", "CommentLine" },
    { "-!!", "NewPage" },
    { "[", "SectionBegin" }
};*/
        private static string[] Tags = { "EmptyLine" , "ChordLine" , "TextLine", "SectionBegin", "CommentLine", "ColumnBreak", "PageBreak" };

        private static Dictionary<string, string> ChordProKeyReplacement = new()
{
    { "title", "{SongTitle}" },
    { "author", "{SongAuthor}" },
};

        private static Dictionary<string, string> ChordProSectionTags = new()
{
    { "V", "Verse" },
    { "B", "Bridge" },
    { "C", "Chorus" }
};

        private static (Dictionary<string, string>, List<string>) _convertSong(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), "Input text cannot be null.");
            }
            else
            {
                //DeEscape
                text = WebUtility.HtmlDecode(text);
                Dictionary<string, string> xmlContent = GetXmlElementContent(text);
                List<string> lyricslines = new(stringToLines(xmlContent["lyrics"]));
                xmlContent = ReplaceKeys(xmlContent, ChordProKeyReplacement);
                return (xmlContent, lyricslines);
            }
        }

        public static (Dictionary<string, string>, List<(string tag, string text)>) convertToDocx(string text)
        {
            Dictionary<string, string> xmlContent = [];
            List<string> lines = [];
            (xmlContent, lines) = _convertSong(text);
            List<(string tag, string text)> fullLyrics = [];

            foreach (string line in lines)
            {
                //string lineT = line.Trim();
                if (line == "")
                {
                    fullLyrics.Add(("EmptyLine", ""));
                }
                else
                {
                    char firstchar = line[0];
                    if (firstchar == '.')
                    {
                        fullLyrics.Add(("ChordLine", line.Substring(1)));
                    }
                    else if (firstchar == ' ')
                    {
                        fullLyrics.Add(("TextLine", line.Substring(1)));
                    }
                    else if (firstchar == '[')
                    {
                        fullLyrics.Add(("SectionBegin", CheckSectionTag(line)));
                    }
                    else if (firstchar == ';')
                    {
                        fullLyrics.Add(("CommentLine", line.Substring(1)));
                    }
                    else if (line.Substring(0, 3) == "---")
                    {
                        fullLyrics.Add(("ColumnBreak", ""));
                    }
                    else if (line.Substring(0, 3) == "-!!")
                    {
                        fullLyrics.Add(("PageBreak", ""));
                    }
                }
            }
            return (xmlContent, fullLyrics);
        }

        // Method to check the section tag and return the corresponding name
        public static string CheckSectionTag(string line)
        {
            // Use a regex to match patterns like [V], [V1], [C], [C2], etc.
            var match = Regex.Match(line, @"^\[(\w)(\d*)\]");

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


        public static (Dictionary<string, string>, string) convertToChordPro(string text)
        {
            Dictionary<string, string> xmlContent = [];
            List<string> lines = [];
            (xmlContent, lines) = _convertSong(text);

            //Investigate the lyrics lines
            int cntLines = 0;
            string chords = "";
            bool inChorus = false;
            List<string> newSong = [];

            //Dictionary<string, string> body = [];

            while (cntLines < lines.Count)
            {
                //Is it a chord line + lyrics combination??
                if (cntLines < lines.Count - 1 && lines[cntLines].StartsWith('.') && lines[cntLines + 1].StartsWith(' '))
                {
                    //It is a chord line + lyrics combination
                    chords = lines[cntLines].Remove(0, 1);

                    string lyric = lines[cntLines + 1].Trim();
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
                            if (chord != "")
                            {
                                int j = i - chord.Length;
                                if (j < 0) j = 0;       //insert chord @ beginning of the line
                                if (i - chord.Length + offset < lyric.Length)
                                    lyric = lyric.Substring(0, j + offset) + "[" + chord + "]" + lyric.Substring(j + offset);
                                else
                                    lyric += chord;
                                offset += chord.Length + 2;
                                chord = "";
                            }
                        }
                    }
                    chords = "";
                    newSong.Add(lyric);
                    cntLines += 2;
                }
                else
                {
                    //Isolated Line
                    string line = lines[cntLines].Trim();
                    if (line.StartsWith("."))
                    {
                        //Chorus without following text line = isolated chord line
                        chords = line.Remove(0, 1);
                        //Surround Chords with []
                        string[] ss = chords.Split(' ');
                        string allchords = "";
                        for (int k = 0; k < ss.Length; k++)
                        {
                            if (ss[k] != "")
                            {
                                allchords += "[" + ss[k] + "] ";
                            }
                        }
                        newSong.Add(allchords);
                    }
                    else if (line.StartsWith("["))
                    {
                        bool skipComment = false;
                        //New Section
                        if (inChorus)
                        {
                            //Close Chorus section
                            newSong.Add("{eoc}");
                            inChorus = false;
                        }
                        if (line.Trim().ToLower().StartsWith("[c"))
                        {
                            //[C ... chorus starts
                            newSong.Add("{soc}");
                            inChorus = true;
                            //Dont write as comment whaterver in this line is
                            skipComment = true;
                        }
                        if (line.Trim().ToLower().StartsWith("[v"))
                        {
                            //[V ... verse
                        }

                        if (!skipComment)
                        {
                            line = line.Replace("[", "");
                            line = line.Replace("]", "");
                            addMakeCommentString(ref newSong, line);
                        }
                    }
                    else if (line.StartsWith(";"))
                    {
                        //Comment
                        if (line.Length > 0)
                        {
                            addMakeCommentString(ref newSong, line.Substring(1));
                        }
                    }
                    else if (line.StartsWith("---"))
                    {
                        //New Column
                        newSong.Add("{column_break}");
                    }
                    else if (line.StartsWith("-!!"))
                    {
                        //New Page
                        newSong.Add("{new_page}");
                        //newSong.Add("{new_physical_page}");
                    }
                    else if (line == "")
                    {
                        //Line break = empty line
                        newSong.Add("{comment:.}");
                    }
                    else
                    {
                        addMakeCommentString(ref newSong, line);
                        chords = "";
                    }
                    cntLines++;
                }
            }

            StringBuilder newSongBuilder = new();
            StringWriter swnewSong = new(newSongBuilder);
            foreach (string s in newSong)
                swnewSong.WriteLine(s);

            return (xmlContent, newSongBuilder.ToString());
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
