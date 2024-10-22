using System.Reflection;
using System.Text.RegularExpressions;

namespace ChordSheetConverter
{
    public class CChordSheetLine
    {
        public static readonly string[] line_separators = ["\r\n", "\n"];
        //public static readonly string nonBreakingSpace = "\u00A0";
        //public static readonly char nonBreakingSpaceChar = '\u00A0';
        public static readonly string nonBreakingSpace = " "; //Not really necessary
        public static readonly char nonBreakingSpaceChar = ' ';


        public CChordSheetLine(enLineType lineType, string line)
        {
            this.lineType = lineType;
            this.line = line;
        }

        public CChordSheetLine(string line)
        {
            this.line = line;
            lineType = getLineType(line);
        }

        public enum enLineType
        {
            Unknown,
            ChordLine,
            TextLine,
            EmptyLine,
            CommentLine,
            xmlElement,
            xmlTagOpenTag,
            xmlTagClosingTag,
            SectionBegin,
            SectionEnd,
            ColumnBreak,
            PageBreak,
        }

        public  enLineType lineType { get; set; }
        public string line {  get; set; }

        public static enLineType getLineType(string line)
        {
            if (CBasicConverter.isChordLine(line) != null)
                return enLineType.ChordLine;

            //"<title>Blabla</title>"
            if (line.Contains('<') && line.Contains("/<"))
                return enLineType.xmlElement;

            if (line.Contains("/<"))
                return enLineType.xmlTagClosingTag;

            if (line.Contains('<'))
                return enLineType.xmlTagOpenTag;

            int printableChars = countPrintableCharacters(line);
            if (printableChars > 0)
                return enLineType.TextLine;

            return enLineType.EmptyLine;
        }

        private static int countPrintableCharacters(string input)
        {
            // Initialize the count to 0
            int printableCharCount = 0;

            // Loop through each character in the string
            foreach (char c in input)
            {
                // Check if the character is printable and not a non-breaking space
                if (!char.IsControl(c) && !char.IsWhiteSpace(c) && c != nonBreakingSpaceChar)
                {
                    // Increment the count for printable characters
                    printableCharCount++;
                }
            }

            return printableCharCount;
        }
    }
}
