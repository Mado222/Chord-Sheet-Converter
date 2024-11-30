using static ChordSheetConverter.CScales;

namespace ChordSheetConverter
{
    public class CAllConverters
    {
        public enum FileFormatTypes
        {
            ChordPro,
            UltimateGuitar,
            OpenSong,
            DOCX
        }

        // Dictionary mapping FileFormatTypes to their respective file extensions
        private static readonly Dictionary<FileFormatTypes, string[]> _fileExtensions = new()
    {
        { FileFormatTypes.UltimateGuitar, new[] { ".txt" } },
        { FileFormatTypes.OpenSong, new[] { ".xml", "*"} },
        { FileFormatTypes.ChordPro, new[] { ".cho", ".chopro" } },
        { FileFormatTypes.DOCX, new[] { ".docx"}}
    };

        public static Dictionary<FileFormatTypes, string[]> FileExtensions => _fileExtensions;


        private readonly Dictionary<FileFormatTypes, IChordSheetAnalyzer> AllFormats = new()
        {
    { FileFormatTypes.UltimateGuitar, new CUltimateGuitar() },
    { FileFormatTypes.OpenSong, new COpenSong() },
    { FileFormatTypes.ChordPro, new CChordPro() },
    { FileFormatTypes.DOCX, new CDocxFormatter() }
    // You could also add more formats like DOCX later if needed
};

        // Function to replace an object in the dictionary with a new instance based on formatType
        public void ReplaceConverterWithNewObject(FileFormatTypes formatType)
        {
            // Replace the object in the dictionary with a new instance based on the formatType
            AllFormats[formatType] = formatType switch
            {
                FileFormatTypes.UltimateGuitar => new CUltimateGuitar(),
                FileFormatTypes.OpenSong => new COpenSong(),
                FileFormatTypes.ChordPro => new CChordPro(),
                FileFormatTypes.DOCX => new CDocxFormatter(),
                _ => throw new ArgumentException("Invalid FileFormatType"),
            };
            Console.WriteLine($"Replaced converter for {formatType} with a new object.");
        }

        public IChordSheetAnalyzer GetConverter(FileFormatTypes format)
        { 
            return AllFormats[format];
        }
        public List<CChordSheetLine> Analyze(FileFormatTypes formatType, string text)
        {
            var res = AllFormats[formatType].Analyze(text);
            return res;
        }

        public List<CChordSheetLine> Analyze(FileFormatTypes formatType, string[] lines)
        {
            return AllFormats[formatType].Analyze(lines);
        }

        public string Build(FileFormatTypes formatType, List<CChordSheetLine> chordSheetLines)
        {
            return AllFormats[formatType].Build (chordSheetLines);
        }

        public (string newSong, List<CChordSheetLine> chordSheetLines) Convert(FileFormatTypes sourceFormatType, FileFormatTypes targetFormatType, string text)
        {
            List<CChordSheetLine> chordSheetLines = AllFormats[sourceFormatType].Analyze(text);
            if (sourceFormatType != targetFormatType)
                ReplaceConverterWithNewObject(targetFormatType);
            AllFormats[targetFormatType].CopyPropertiesFrom(GetConverter(sourceFormatType));
            string newSong = AllFormats[targetFormatType].Build(chordSheetLines);
            return (newSong, chordSheetLines);
        }

        public void CopyPropertiesFrom<TSource>(FileFormatTypes targetFormatType, TSource source)
        {
            AllFormats[targetFormatType].CopyPropertiesFrom(source);
        }

        public string UpdateTags(FileFormatTypes sourceFormatType, string textIn, bool showInputWindow = true)
        {
            if (showInputWindow)
            {
                var inputWindow = new InputWindow(AllFormats[sourceFormatType]);
                if (inputWindow.ShowDialog() == true)
                {
                    // The docxFormatter object now contains the updated values from the input window.
                }
            }
            return AllFormats[sourceFormatType].UpdateTags(textIn);
        }

        public (CChordCollection chords, string lyrics) ExtractChords(FileFormatTypes sourceFormatType, string line)
        {
            return AllFormats[sourceFormatType].ExtractChords(line);
        }

        public string [] ExtractAllChords(FileFormatTypes sourceFormatType, string[] text)
        {
            List <string> allchords = [];

            foreach (string line in text)
            {
                (CChordCollection chords, string _) = ExtractChords(sourceFormatType, line);

                if (chords != null && chords.Any())
                {
                    foreach (CChord chord in chords)
                        allchords.Add(chord.Chord);
                }
            }
            return [.. allchords];
        }
    }
}
