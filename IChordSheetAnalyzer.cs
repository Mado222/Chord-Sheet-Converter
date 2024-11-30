using static ChordSheetConverter.CAllConverters;
using static ChordSheetConverter.CScales;

namespace ChordSheetConverter
{
    public interface IChordSheetAnalyzer
    {
        string Title { get; set; }
        string Composer { get; set; }
        string Copyright { get; set; }
        string Capo { get; set; }
        string Tempo { get; set; }
        string Time { get; set; }
        string Key { get; set; }

        // Properties for ChordPro
        //string sortTitle { get; set; } 
        string SubTitle { get; set; }
        string Artist { get; set; }
        string Lyricist { get; set; }
        string Album { get; set; }
        string Year { get; set; }
        string Duration { get; set; }

        List<CChordSheetLine> Analyze(string text);
        List<CChordSheetLine> Analyze(string[] lines);
        string Build(List<CChordSheetLine> chordSheetLines);
        void CopyPropertiesFrom<TSource>(TSource source);
        string UpdateTags(string textIn);

        Dictionary<string, string> PropertyMapDisplayNames { get; }
        Dictionary<string, string> PropertyMapTags { get; }

        string Transpose(string textIn, TranspositionParameters? parameters = null, int? steps = null);

        string[] Transpose(string[] linesIn, TranspositionParameters? parameters = null, int? steps = null);

        (CChordCollection chords, string lyrics) ExtractChords(string line);

        string ConverToNashville(string text, string key, ScaleType scaleType = ScaleType.Major);
    }
}
