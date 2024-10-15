using System.Text;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using System.Windows;


namespace ChordSheetConverter
{
    public class CMusicSheetToRTF
    {
        //public static readonly CFont ftChords = new(new FontFamily("Courier New"), 12, FontWeights.Normal, FontStyles.Italic, Brushes.Blue);
        //public static readonly CFont ftLyrics = new(new FontFamily("Courier New"), 12, FontWeights.Normal, FontStyles.Normal, Brushes.Black);

        // Converts a string into an RTF formatted line with a specific Font and Color
        public static string ConvertLineToRtf(string inputText, CFont cfont, int colorIndex)
        {
            StringBuilder rtf = new();

            // Start the actual text content
            // Font size in RTF is measured in half-points, so multiply by 2
            //rtf.Append($@"\f0\fs{(int)(cfont.SizeInPoints * 2)} "); // Set font and size

            // Handle bold and italic styles
            if (cfont.FontWeight==FontWeights.Bold)
            {
                rtf.Append(@"\b ");
            }

            if (cfont.FontStyle == FontStyles.Italic)
            {
                rtf.Append(@"\i ");
            }

            // Apply the color (use the provided color index)
            rtf.Append($@"\cf{colorIndex} ");  // Apply the color from the color table

            // Add the input text
            rtf.Append(inputText);

            // Close bold and italic styles if applied
            if (cfont.FontWeight == FontWeights.Bold)
            {
                rtf.Append(@"\b0 ");
            }

            if (cfont.FontStyle == FontStyles.Italic)
            {
                rtf.Append(@"\i0 ");
            }

            return rtf.ToString();
        }


        // Function to extract chords and text from a ChordPro line and format them into chord and text lines
        public static (string chordLine, string textLine) GetChordAndTextLines(string input)
        {
            string chordLine = "";
            string textLine = "";

            int currentPos = 0;  // Tracks the current position in the text

            // Use a simplified regular expression to match anything between square brackets
            Regex regex = new(@"\[([^\]]+)\]");  // Match anything between '[' and ']'
            var matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                string chord = match.Groups[1].Value; // Extract the chord (anything between brackets)
                int chordPos = match.Index;           // Get the position of the chord in the input string

                // Add the text before the chord to the text line
                string textBeforeChord = input[currentPos..chordPos];
                textLine += textBeforeChord;

                // Add spaces to the chord line to align with the text, considering the length of the chord markers
                chordLine += new string(' ', textBeforeChord.Length) + chord;

                // Move the current position past the chord (and the removed brackets)
                currentPos = chordPos + match.Length;  // match.Length includes the brackets
            }

            // Add any remaining text after the last chord
            textLine += input[currentPos..];

            // Return the formatted chord and text lines
            return (chordLine, textLine);
        }


        public static string ConvertToRTF(string[] inputLines)
        {
            StringBuilder rtf = new ();
            // Start RTF document
            rtf.Append(@"{\rtf1\ansi\deff0 ");

            // Define the color table (color 1 is blue, color 2 is black)
            rtf.Append(@"{\colortbl ;\red0\green0\blue255;\red0\green0\blue0;}");

            // Define the font table (f0 is Courier New)
            rtf.Append(@"{\fonttbl {\f0 Courier New;}}");

            // Process each input line
            foreach (string inputLine in inputLines)
            {
                (string chordLine, string lyricsLine) = GetChordAndTextLines(inputLine);

                // Convert and append chord line with paragraph break
                //rtf.Append(ConvertLineToRtf(chordLine, ftChords, 0));
                rtf.Append(@"\par "); // Add RTF paragraph break

                // Convert and append lyrics line with paragraph break
                //rtf.Append(ConvertLineToRtf(lyricsLine, ftLyrics, 1));
                rtf.Append(@"\par "); // Add RTF paragraph break
            }

            // Close the RTF string
            rtf.Append(@"}");
            return rtf.ToString();
        }
    }
}

