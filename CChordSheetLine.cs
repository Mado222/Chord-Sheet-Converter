namespace ChordSheetConverter
{
    public class CChordSheetLine
    {
        public static readonly string[] line_separators = ["\r\n", "\n"];
        //public static readonly string nonBreakingSpace = "\u00A0";
        //public static readonly char nonBreakingSpaceChar = '\u00A0';
        public static readonly string nonBreakingSpace = " "; //Not really necessary
        public static readonly char nonBreakingSpaceChar = ' ';


        public CChordSheetLine(EnLineType lineType, string line)
        {
            this.LineType = lineType;
            this.Line = line;
        }

        public CChordSheetLine(string line)
        {
            this.Line = line;
            LineType = GetLineType(line);
        }

        public enum EnLineType
        {
            Unknown,
            ChordLineVerse,
            ChordLineChorus,
            ChordLineBridge,
            TextLine,
            TextLineVerse,
            TextLineChorus,
            TextLineBridge,
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

        public EnLineType LineType { get; set; }
        public string Line { get; set; }

        public static EnLineType GetLineType(string line)
        {
            if (CBasicConverter.IsChordLine(line) != null)
                return EnLineType.ChordLineVerse;

            //"<title>Blabla</title>"
            if (line.Contains('<') && line.Contains("/<"))
                return EnLineType.xmlElement;

            if (line.Contains("/<"))
                return EnLineType.xmlTagClosingTag;

            if (line.Contains('<'))
                return EnLineType.xmlTagOpenTag;

            int printableChars = CountPrintableCharacters(line);
            if (printableChars > 0)
                return EnLineType.TextLine;

            return EnLineType.EmptyLine;
        }

        private static int CountPrintableCharacters(string input)
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
