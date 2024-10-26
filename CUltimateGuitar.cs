using System.Collections.Generic;
using static ChordSheetConverter.CScales;
using EnLineType = ChordSheetConverter.CChordSheetLine.EnLineType;

namespace ChordSheetConverter
{
    public class CUltimateGuitar: CBasicConverter
    {

        public override List<CChordSheetLine> Analyze(string[] lines) //returns chordSheetLines
        {
            List<CChordSheetLine> ret = [];
            foreach (string l in  lines)
            {
                var res = IsChordLine(l);
                if (res is not null)
                {
                    //Chord line
                    ret.Add(new CChordSheetLine(EnLineType.ChordLine, l));
                }
                else
                {
                    ret.Add(new CChordSheetLine(EnLineType.TextLine, l));
                }
            }
            return ret;
        }

        public override List<CChordSheetLine> Analyze(string text) //returns chordSheetLines
        {
            return Analyze (StringToLines(text));
        }

        // Define a class to hold section information
        private class SectionInfo(string startTag, string endTag, string comment)
        {
            public string StartTag { get; } = startTag;
            public string EndTag { get; } = endTag;
            public string Comment { get; } = comment;
        }
    }
}