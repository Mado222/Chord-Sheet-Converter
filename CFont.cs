using System.Windows;
using System.Windows.Media;

namespace ChordSheetConverter
{ 
    public class CFont
    {
        // Font family property
        public FontFamily FontFamily { get; set; }

        // Font size property
        public double FontSize { get; set; }

        // Font weight property (e.g., Bold, Normal)
        public FontWeight FontWeight { get; set; }

        // Font style property (e.g., Italic, Normal)
        public FontStyle FontStyle { get; set; }

        // Foreground (color of the text)
        public Color FontColor { get; set; }

        // Constructor with default values
        public CFont(FontFamily fontFamily, double fontSize, FontWeight fontWeight, FontStyle fontStyle, Color color)
        {
            FontFamily = fontFamily;
            FontSize = fontSize;
            FontWeight = fontWeight;
            FontStyle = fontStyle;
            FontColor = color;
        }
    }
}
