using System.Collections.Generic;
using static ChordSheetConverter.CScales;
using enLineType = ChordSheetConverter.CChordSheetLine.enLineType;

namespace ChordSheetConverter
{
    public class CUltimateGuitar: CBasicConverter, IChordSheetAnalyzer
    {
        public Dictionary<string, string> propertyMapTags { get; } = [];

        public static List<string> convertToNashville(List<string> chords, string key, ScaleType scaleType = ScaleType.Major)
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

        public override List<CChordSheetLine> analyze(string[] lines) //returns chordSheetLines
        {
            List<CChordSheetLine> ret = [];
            foreach (string l in  lines)
            {
                var res = isChordLine(l);
                if (res is not null)
                {
                    //Chord line
                    ret.Add(new CChordSheetLine(enLineType.ChordLine, l));
                }
                else
                {
                    ret.Add(new CChordSheetLine(enLineType.TextLine, l));
                }
            }
            return ret;
        }

        public override List<CChordSheetLine> analyze(string text) //returns chordSheetLines
        {
            return analyze (stringToLines(text));
        }

        // Define a class to hold section information
        private class SectionInfo
        {
            public string startTag { get; }
            public string endTag { get; }
            public string comment { get; }

            public SectionInfo(string startTag, string endTag, string comment)
            {
                this.startTag = startTag;
                this.endTag = endTag;
                this.comment = comment;
            }
        }
   }
}