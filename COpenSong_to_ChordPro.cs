using System.Net;
using System.Text;
using System.IO;

namespace ChordSheetConverter
{
    public static class OpenSongToChordPro
    {
        public static string convertSong(string text)
        {
            if (text is null)
            {
                throw new ArgumentNullException(nameof(text), "Input text cannot be null.");
            }
            else
            {

                //DeEscape
                text = WebUtility.HtmlDecode(text);

                //grab the metadata first
                List<string> newSong = [];


                string[][] tags = [ ["title","title"],
                                  ["author","subtitle"],
                                  ["key","comment"],
                                  ["capo","comment"],
                                  ["tempo","comment"],
                                  ["ccli","subtitle"],
                                  ["presentation","comment"],
                                  ["copyright","comment"],
                                  ["theme","comment"]
                                      ];
                //find all tags in <>
                foreach (string[] tag in tags)
                {
                    string data = tag[0];
                    int begin = text.ToLower().IndexOf("<" + data) + 2 + data.Length;
                    int end = text.ToLower().IndexOf("</" + data);
                    if (begin >= 0 && end >= 0)
                    {
                        string temp = text.Substring(text.ToLower().IndexOf("<" + data) + 2 + data.Length);
                        temp = temp[..temp.ToLower().IndexOf("</" + data)];
                        if (!temp.Contains("print=\"false\""))
                        {
                            if (temp != "")
                            {
                                //Only add not empty tags
                                newSong.Add("{" + tag[1] + ":" + temp + "}");
                            }
                        }
                    }
                }

                text = text.Substring(text.ToLower().IndexOf("<lyrics>") + 8);
                text = text.Substring(0, text.ToLower().IndexOf("</lyrics"));
                var lines = text.Replace("\r", "").Split('\n').ToList();
                //lines.RemoveAt(0);
                string chords = "";
                bool inChorus = false;

                //Investigate the lyrics lines
                int cntLines = 0;
                while (cntLines < lines.Count)
                {
                    if (cntLines < lines.Count - 1 && lines[cntLines].StartsWith(".") && lines[cntLines + 1].StartsWith(" "))
                    {
                        //It is a chord line + lyrics combination
                        chords = lines[cntLines].Remove(0, 1);

                        string result = lines[cntLines + 1].Trim();
                        string chord = "";
                        int offset = 0;
                        //Make chords string and lyrics string equally long if chords string is shorter
                        if (chords.Length > result.Length)
                        {
                            for (int i = result.Length; i <= chords.Length; i++)
                            {
                                result += " ";
                            }
                        }
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
                                    if (i - chord.Length + offset < result.Length)
                                        result = result.Substring(0, j + offset) + "[" + chord + "]" + result.Substring(j + offset);
                                    else
                                        result += chord;
                                    offset += chord.Length + 2;
                                    chord = "";
                                }
                            }
                        }
                        chords = "";
                        newSong.Add(result);
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

                StringBuilder newSongBuilder = new ();
                StringWriter swnewSong = new(newSongBuilder);
                foreach (string s in newSong)
                    swnewSong.WriteLine(s);

                return newSongBuilder.ToString();
            }
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
