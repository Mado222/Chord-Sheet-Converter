using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static ChordSheetConverter.CScales;

namespace ChordSheetConverter
{
    public class UltimateGuitarToChordpro
    {
        private static readonly string[] line_separators = ["\r\n", "\n"];

        public static List<string> ConvertToNashville(List<string> chords, string key, ScaleType scaleType = ScaleType.Major)
        {

            if (!chromaticScale.Contains(key))
            {
                throw new ArgumentException($"Unsupported key: {key}");
            }
            List<string> scale = GetScaleNotes(scaleType, key);
            List<string> nashvilleChords = [];

            foreach (var chord in chords)
            {
                // Split chords with bass notes (e.g., G/B)
                string[] chordParts = chord.Split('/');
                string rootChord = chordParts[0];
                string? bassNote = chordParts.Length > 1 ? chordParts[1] : null;

                // Convert the root chord to Nashville number
                for (int i = 0; i < scale.Count; i++)
                {
                    if (rootChord.StartsWith(scale[i]))
                    {
                        string nashvilleNumber = (i + 1).ToString();
                        string extensions = rootChord[scale[i].Length..]; // Chord extensions (e.g., "maj7")

                        // Handle bass notes
                        if (bassNote != null)
                        {
                            for (int j = 0; j < scale.Count; j++)
                            {
                                if (bassNote == scale[j])
                                {
                                    nashvilleChords.Add($"{nashvilleNumber}{extensions}/{(j + 1)}");
                                    break;
                                }
                            }
                        }
                        else
                        {
                            nashvilleChords.Add($"{nashvilleNumber}{extensions}");
                        }
                        break;
                    }
                }
            }

            return nashvilleChords;
        }

        public static (List<string> chords, List<int> positions) ExtractChordsWithPositions(string inputString)
        {
            List<string> chords = [];
            List<int> positions = [];

            // Regex to match non-whitespace sequences in the input string
            Regex regex = new(@"\S+");
            MatchCollection matches = regex.Matches(inputString);

            // Loop through each match and capture the group and its position
            foreach (Match match in matches)
            {
                chords.Add(match.Value);        // Add the matched word (group) to the chords list
                positions.Add(match.Index);     // Add the starting index of the group to the positions list
            }

            return (chords, positions);  // Return both the chords and positions as a tuple
        }

        public static bool IsValidChord(string chord)
        {
            string chordPattern = @"^[A-G][#b]?m?(maj|sus|dim|aug)?[0-9]?(add[0-9])?(/[A-G][#b]?)?$";
            return Regex.IsMatch(chord, chordPattern);
        }

        public static (List<string> chords, List<int> positions)? DetectChordsOrText(string inputString)
        {
            var res = ExtractChordsWithPositions(inputString);

            foreach (var group in res.chords)
            {
                if (!IsValidChord(group))
                {
                    return null; // Return null if any group is not a valid chord
                }
            }

            return res; // Return chords and positions if all are valid chords
        }

        public static string MergeTextAndChords(string textLine, List<string> chords, List<int> positions)
        {
            StringBuilder result = new ();
            int textIndex = 0;
            int chordsIndex = 0;

            while (textIndex < textLine.Length)
            {
                if (chordsIndex < positions.Count && textIndex == positions[chordsIndex])
                {
                    // Insert the chord at this position
                    result.Append($"[{chords[chordsIndex]}]");
                    chordsIndex++;
                }
                result.Append(textLine[textIndex]);
                textIndex++;
            }

            return result.ToString();
        }

        public static string FormatChordLine(List<string> chords)
        {
            if (chords != null && chords.Count > 0)
            {
                return string.Join("  ", chords.Select(chord => $"[{chord}]"));
            }
            return string.Empty;
        }

        public static void ConvertUGFileToChordPro(string inputFile, string outputFile)
        {
            string songName = Path.GetFileNameWithoutExtension(inputFile);
            string[] outputLines = ConvertUGToChordPro(File.ReadAllLines(inputFile), songName);
            File.WriteAllLines(outputFile, outputLines);
        }

        public static List<string[]> ExtractChords(string[] inputLines)
        {
            List<string[]> ret = [];
            foreach (string line in inputLines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var chordInfo = DetectChordsOrText(line);
                    if (chordInfo != null)
                    {
                        ret.Add([.. chordInfo.Value.chords]);
                    }
                }
            }
            return ret;
        }

        // Convert the input file to the output file in C#
        public static string[] ConvertUGToChordPro(string[] inputLines, string songName, string? key = null)
        {
            List<string> outputLines =
        [
            $"{{title: {songName}}}" // Add the title line as the first line
        ];

            int i = 0;

            while (i < inputLines.Length)
            {
                string line = inputLines[i].TrimEnd();

                if (line == "")
                {
                    // If it's an empty line, just keep it as is
                    outputLines.Add("");
                    i++;
                    continue;
                }

                // Check if this is a chord line
                var chordInfo = DetectChordsOrText(line);  // Assuming DetectChordsOrText exists and returns chords and positions

                if (chordInfo != null)
                {
                    List<string> chords = chordInfo.Value.chords;
                    List<int> positions = chordInfo.Value.positions;

                    // Optional: Convert the chords to Nashville numbering system
                    if (key is not null)
                    {
                        chords = ConvertToNashville(chords, key);  // Assuming you want to convert to Nashville in key C
                    }

                    if (i + 1 < inputLines.Length && !string.IsNullOrWhiteSpace(inputLines[i + 1]))
                    {
                        // If there's a text line after the chord line
                        string textLine = inputLines[i + 1].Trim();
                        string mergedLine = MergeTextAndChords(textLine, chords, positions);
                        outputLines.Add(mergedLine);
                        i += 2; // Skip both the chord and text line
                    }
                    else
                    {
                        // If the chord line is followed by an empty line, format it
                        string formattedChordLine = FormatChordLine(chords);
                        outputLines.Add(formattedChordLine);
                        i++;
                    }
                }
                else if (i > 0 && string.IsNullOrWhiteSpace(inputLines[i - 1]) && !string.IsNullOrWhiteSpace(line))
                {
                    // If the previous line is empty and the current line is text, keep the text line
                    outputLines.Add(line);
                    i++;
                }
                else
                {
                    // If it's not a chord line, just append the line as is
                    outputLines.Add(line);
                    i++;
                }
            }
            return [.. outputLines];
        }

        // Convert the input file to the output file in C#
        public static string[] ConvertUGToOpenSong(string[] inputLines, string songName, string? key = null, string? author = null)
        {
            List<string> outputLines =
        [
            "<?xml version=\"1.0\" encoding=\"UTF-8\"?>",
            "<song>",
            $"<title>{songName}</title>",
            "<lyrics>"
        ];

            bool prev_waschordline = false;
            foreach (string line in inputLines)
            {
                var ci = DetectChordsOrText(line);
                if (ci != null && ci.Value.chords.Count > 0)
                {
                    //chord line
                    outputLines.Add(("." + line).Trim());
                    prev_waschordline = true;
                }
                else
                {
                    if (prev_waschordline)
                    {
                        //Chord line
                        outputLines.Add((" " + line).Trim());
                        prev_waschordline = false;
                    }
                    else if (line == "")
                    {
                        //Empty line
                        outputLines.Add(line.Trim());
                    }
                    else
                    {
                        //Any other textline
                        outputLines.Add((";" + line).Trim());
                    }
                }
            }

            //close xml tags
            outputLines.Add("</lyrics >");
            if (author != null)
            {
                outputLines.Add($"<author>{author}</author>");
            }
            outputLines.Add("</song >");
            return outputLines.ToArray();
        }

        // Define a class to hold section information
        private class SectionInfo
        {
            public string StartTag { get; }
            public string EndTag { get; }
            public string Comment { get; }

            public SectionInfo(string startTag, string endTag, string comment)
            {
                StartTag = startTag;
                EndTag = endTag;
                Comment = comment;
            }
        }

        // Dictionary to map section types to ChordPro tags
        private static readonly Dictionary<string, SectionInfo> sectionMappings = new()
    {
        { "v", new SectionInfo("{start_of_verse}", "{end_of_verse}", "Verse") },
        { "chorus", new SectionInfo("{start_of_chorus}", "{end_of_chorus}", "Chorus") },
        { "solo", new SectionInfo("{start_of_solo}", "{end_of_solo}", "Solo") },
        { "intro", new SectionInfo("{start_of_intro}", "{end_of_intro}", "Intro") },
    };

        // Track active section
        private static string? activeSection = null;

        public static string ConvertOpenSongToChordPro(string openSongFilePath)
        {
            string chordLine = "";
            string textLine = "";

            // Load the OpenSong XML file
            XElement songXml = XElement.Load(openSongFilePath);

            // Extract the title and lyrics from the XML
            string title = songXml.Element("title")?.Value ?? "Untitled Song";
            string lyrics = songXml.Element("lyrics")?.Value ?? string.Empty;

            // Create a StringBuilder for the ChordPro output
            StringBuilder chordProOutput = new();

            // Add title to the ChordPro output
            chordProOutput.AppendLine($"{{title: {title}}}");
            chordProOutput.AppendLine();

            // Split the lyrics into lines
            string[] lines = lyrics.Split(line_separators, StringSplitOptions.RemoveEmptyEntries);

            bool prevLineWasChord = false;

            // Close any active section
            void CloseActiveSection()
            {
                if (activeSection != null)
                {
                    var section = sectionMappings[activeSection];
                    chordProOutput.AppendLine(section.EndTag);
                    chordProOutput.AppendLine();
                    activeSection = null;
                }
            }

            foreach (var line in lines)
            {
                string trimmedLine = line;

                // Handle comments (starting with ';')
                if (trimmedLine.StartsWith(';'))
                {
                    //CloseActiveSection();
                    trimmedLine = trimmedLine.Substring(1).Trim();
                    chordProOutput.AppendLine($"{{comment_italic: {trimmedLine}}}");
                    //chordProOutput.AppendLine("- " + trimmedLine);
                    continue;
                }

                // Handle chord lines (starting with a dot)
                if (trimmedLine.StartsWith('.'))
                {
                    if (prevLineWasChord)
                    {
                        var (groups, positions) = ExtractChordsWithPositions(chordLine);
                        chordLine = FormatChordLine(groups);
                        chordProOutput.AppendLine(chordLine);
                        prevLineWasChord = false;
                    }

                    chordLine = trimmedLine.Substring(1);
                    if (chordLine.Length == 0)
                        chordProOutput.AppendLine(" ");

                    prevLineWasChord = true;
                    continue;
                }

                // Handle text lines (starting with a space)
                if (trimmedLine.StartsWith(' '))
                {
                    textLine = trimmedLine.Substring(1).Trim();
                    if (textLine.Length > 0 && prevLineWasChord)
                    {
                        var (groups, positions) = ExtractChordsWithPositions(chordLine);
                        string combinedLine = MergeTextAndChords(textLine, groups, positions);
                        chordProOutput.AppendLine(combinedLine);
                        prevLineWasChord = false;
                    }
                    continue;
                }

                if (prevLineWasChord)
                {
                    var (groups, positions) = ExtractChordsWithPositions(chordLine);
                    chordLine = FormatChordLine(groups);
                    chordProOutput.AppendLine(chordLine);
                    prevLineWasChord = false;
                    continue;
                }

                // Handle special sections like [V1], [Chorus], [Solo]
                if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith(']'))
                {
                    string section = trimmedLine.Trim('[', ']').ToLower();
                    string sectionKey = section;
                    if (section[0] == 'v')
                        sectionKey = "v"; // Handle cases like "v1"

                    if (sectionMappings.ContainsKey(sectionKey))
                    {
                        CloseActiveSection();  // Close any previously active section

                        var sectionInfo = sectionMappings[sectionKey];
                        chordProOutput.AppendLine(sectionInfo.StartTag);
                        string comment = sectionInfo.Comment;
                        if (section[0] == 'v')
                            comment = "Verse " + section[1];

                        chordProOutput.AppendLine($"{{comment: {comment}}}");

                        activeSection = sectionKey;
                    }
                    continue;
                }

                if (trimmedLine.StartsWith("---"))
                {
                    chordProOutput.AppendLine($"{{column_break}}");
                    continue;
                }


                // Append any remaining text
                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    chordProOutput.AppendLine(trimmedLine);
                }
            }

            // Close any remaining open section
            if (prevLineWasChord)
            {
                var (groups, positions) = ExtractChordsWithPositions(chordLine);
                chordLine = FormatChordLine(groups);
                chordProOutput.AppendLine(chordLine);
                prevLineWasChord = false;
            }

            CloseActiveSection();

            return chordProOutput.ToString();
        }

        public static string ConvertOpenSongToChordPro_old(string openSongFilePath)
        {
            string chordLine = "";
            string textLine = "";

            // Load the OpenSong XML file
            XElement songXml = XElement.Load(openSongFilePath);

            // Extract the title and lyrics from the XML
            string title = songXml.Element("title")?.Value ?? "Untitled Song";
            string lyrics = songXml.Element("lyrics")?.Value ?? string.Empty;

            // Create a StringBuilder for the ChordPro output
            StringBuilder chordProOutput = new ();

            // Add title to the ChordPro output
            chordProOutput.AppendLine($"{{title: {title}}}");
            chordProOutput.AppendLine();

            // Split the lyrics into lines
            string[] lines = lyrics.Split(line_separators, StringSplitOptions.RemoveEmptyEntries);

            bool insideVerse = false;
            bool insideChorus = false;
            bool insideSolo = false;
            bool prevLineWasChord = false;

            void endRegions()
            {
                if (insideVerse)
                {
                    chordProOutput.AppendLine("{end_of_verse}");
                    chordProOutput.AppendLine(" ");
                }
                if (insideChorus)
                {
                    chordProOutput.AppendLine("{end_of_chorus}");
                    chordProOutput.AppendLine();
                }
                if (insideSolo)
                {
                    chordProOutput.AppendLine("{end_of_solo}");
                    chordProOutput.AppendLine();
                }
            }


            foreach (var line in lines)
            {
                string trimmedLine = line;//.Trim();

                // Handle comments (starting with ';')
                if (trimmedLine.StartsWith(';'))
                {
                    endRegions();
                    trimmedLine = trimmedLine.Substring(1);
                    chordProOutput.AppendLine($"{{comment: {trimmedLine}}}");
                    continue;
                }

                // Handle chord lines (starting with a dot)
                if (trimmedLine.StartsWith('.'))
                {
                    if (prevLineWasChord)
                    {
                        var (groups, positions) = ExtractChordsWithPositions(chordLine);

                        chordLine = FormatChordLine(groups);
                        chordProOutput.AppendLine(chordLine);
                        prevLineWasChord = false;
                    }

                    chordLine = trimmedLine.Substring(1);
                    if (chordLine.Length == 0)
                        chordProOutput.AppendLine(" ");

                    prevLineWasChord = true;
                    
                    continue;
                }

                // Handle text lines (starting with a space)
                if (trimmedLine.StartsWith(' '))
                {
                    //Text Line
                    textLine = trimmedLine.Substring(1).Trim();
                    if (textLine.Length > 0 && prevLineWasChord)
                    {
                        //Chords and Text
                        var (groups, positions) = ExtractChordsWithPositions(chordLine);
                        string combinedLine = MergeTextAndChords(textLine, groups, positions);
                        chordProOutput.AppendLine(combinedLine);
                        prevLineWasChord = false;
                        continue;
                    }
                }

                if (prevLineWasChord)
                {
                    //No text line for the chord found
                    var (groups, positions) = ExtractChordsWithPositions(chordLine);
                    chordLine = FormatChordLine(groups);
                    chordProOutput.AppendLine(chordLine);
                    prevLineWasChord = false;
                    continue;
                }


                // Handle special sections like [V1], [C], [Solo]
                if (trimmedLine.StartsWith('[') && trimmedLine.EndsWith("]"))
                {
                    string section = trimmedLine.Trim('[', ']');
                    switch (section.ToLower())
                    {
                        case "v1":
                        case "v2":
                        case "v3":
                        case "v4":
                        case "v5":
                            endRegions();
                            chordProOutput.AppendLine("{start_of_verse}");
                            chordProOutput.AppendLine($"{{comment:Verse {section[1]}}}");
                            insideVerse = true;
                            insideChorus = false;
                            insideSolo = false;
                            break;
                        case "c":
                        case "chorus":
                            endRegions();
                            chordProOutput.AppendLine("{start_of_chorus}");
                            chordProOutput.AppendLine($"{{comment:Chorus}}");
                            insideVerse = false;
                            insideChorus= true;
                            insideSolo = false;
                            break;
                        case "solo":
                            endRegions();
                            chordProOutput.AppendLine("{start_of_solo}");
                            chordProOutput.AppendLine($"{{comment:Solo}}");
                            //chordProOutput.AppendLine("{end_of_solo}");
                            insideVerse = false;
                            insideChorus = false;
                            insideSolo = true;
                            break;
                        default:
                            break;
                    }
                    continue;
                }
               
                if (!string.IsNullOrEmpty(trimmedLine))
                {
                    
                    chordProOutput.AppendLine(trimmedLine);
                }
            }

            // Close any remaining
            endRegions();

            return chordProOutput.ToString();
        }
    }
}