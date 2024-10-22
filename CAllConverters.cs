using Microsoft.Office.Interop.Word;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static Dictionary<FileFormatTypes, string[]> fileExtensions => _fileExtensions;


        private Dictionary<FileFormatTypes, IChordSheetAnalyzer> AllFormats = new Dictionary<FileFormatTypes, IChordSheetAnalyzer>
{
    { FileFormatTypes.UltimateGuitar, new CUltimateGuitar() },
    { FileFormatTypes.OpenSong, new COpenSong() },
    { FileFormatTypes.ChordPro, new CChordPro() },
    { FileFormatTypes.DOCX, new CDocxFormatter() }
    // You could also add more formats like DOCX later if needed
};

        // Function to replace an object in the dictionary with a new instance based on formatType
        public void replaceConverterWithNewObject(FileFormatTypes formatType)
        {
            // Replace the object in the dictionary with a new instance based on the formatType
            switch (formatType)
            {
                case FileFormatTypes.UltimateGuitar:
                    AllFormats[formatType] = new CUltimateGuitar();
                    break;

                case FileFormatTypes.OpenSong:
                    AllFormats[formatType] = new COpenSong();
                    break;

                case FileFormatTypes.ChordPro:
                    AllFormats[formatType] = new CChordPro();
                    break;

                case FileFormatTypes.DOCX:
                    AllFormats[formatType] = new CDocxFormatter();
                    break;

                default:
                    throw new ArgumentException("Invalid FileFormatType");
            }

            Console.WriteLine($"Replaced converter for {formatType} with a new object.");
        }

        public IChordSheetAnalyzer getConverter(FileFormatTypes format) 
        { 
            return AllFormats[format];
        }
        public List<CChordSheetLine> analyze(FileFormatTypes formatType, string text)
        {
            var res = AllFormats[formatType].analyze(text);
            return res;
        }

        public List<CChordSheetLine> analyze(FileFormatTypes formatType, string[] lines)
        {
            return AllFormats[formatType].analyze(lines);
        }

        public string build(FileFormatTypes formatType, List<CChordSheetLine> chordSheetLines)
        {
            return AllFormats[formatType].build (chordSheetLines);
        }

        public (string newSong, List<CChordSheetLine> chordSheetLines) convert(FileFormatTypes sourceFormatType, FileFormatTypes targetFormatType, string text)
        {
            List<CChordSheetLine> chordSheetLines  = AllFormats[sourceFormatType].analyze(text);
            replaceConverterWithNewObject(targetFormatType);
            AllFormats[targetFormatType].copyPropertiesFrom(getConverter(sourceFormatType));
            string newSong = AllFormats[targetFormatType].build(chordSheetLines);
            return (newSong, chordSheetLines);
        }

        public void copyPropertiesFrom<TSource>(FileFormatTypes targetFormatType, TSource source)
        {
            AllFormats[targetFormatType].copyPropertiesFrom(source);
        }

        public string updateTags(FileFormatTypes sourceFormatType, string textIn, bool showInputWindow = true)
        {
            if (showInputWindow)
            {
                var inputWindow = new InputWindow(AllFormats[sourceFormatType]);
                if (inputWindow.ShowDialog() == true)
                {
                    // The docxFormatter object now contains the updated values from the input window.
                }
            }
            return AllFormats[sourceFormatType].updateTags(textIn);
        }
    }
}
