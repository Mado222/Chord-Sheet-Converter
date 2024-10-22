using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml.Linq;
using static ChordSheetConverter.CAllConverters;

namespace ChordSheetConverter
{
    public interface IChordSheetAnalyzer
    {
         string title { get; set; } 
         string composer { get; set; } 
         string copyright { get; set; } 
         string capo { get; set; } 
         string tempo { get; set; } 
         string time { get; set; } 
         string key { get; set; } 

        // Properties for ChordPro
         //string sortTitle { get; set; } 
         string subTitle { get; set; } 
         string artist { get; set; } 
         string lyricist { get; set; } 
         string album { get; set; } 
         string year { get; set; } 
         string duration { get; set; } 

        List<CChordSheetLine> analyze(string text);
        List<CChordSheetLine> analyze(string[] lines);
        string build(List<CChordSheetLine> chordSheetLines);
        void copyPropertiesFrom<TSource>(TSource source);
        string updateTags(string textIn);

        Dictionary<string, string> propertyMapDisplayNames { get; }
        Dictionary<string, string> propertyMapTags { get; }
    }

    public class CBasicConverter
    {
        //Common properties
        public string title { get; set; } = "";
        public string composer { get; set; } = "";
        public string copyright { get; set; } = "";
        public string capo { get; set; } = "";
        public string tempo { get; set; } = "";
        public string time { get; set; } = "";
        public string key { get; set; } = "";

        // Properties for ChordPro
        //public string sortTitle { get; set; } = "";
        public string subTitle { get; set; } = "";
        public string artist { get; set; } = "";
        public string lyricist { get; set; } = "";
        public string album { get; set; } = "";
        public string year { get; set; } = "";
        public string duration { get; set; } = "";

        public static readonly Dictionary<string, string> _propertyMapDisplayNames = new()
        {
    { "title", "Song Title" },
    { "author", "Author Name" },
    { "subTitle", "Subtitle" },
    { "composer", "Composer" },
    { "lyricist", "Lyricist" },
    { "copyright", "Copyright" },
    { "year", "Year (Released, Written ...)" },
    { "key", "Key" },
    { "time", "Time Signature" },
    { "tempo", "Tempo" },
    { "capo", "Capo" }
        };

        public Dictionary<string, string> propertyMapDisplayNames { get;  } = _propertyMapDisplayNames;

        private static List<CChordSheetLine> _chordSheetLines = [];

        public static List<CChordSheetLine> chordSheetLines
        {
            get { return _chordSheetLines; }
            set { _chordSheetLines = value ?? []; }  // Null-check
        }

        public static string[] stringToLines(string text) => text.Split(CChordSheetLine.line_separators, StringSplitOptions.None);
        public static string linesToString(string[] lines) => string.Join(Environment.NewLine, lines);

        public static string getLines(List<CChordSheetLine> csLines)
        {
            string ret = "";
            foreach (CChordSheetLine line in csLines)
            {
                ret += line.line + CChordSheetLine.line_separators[0];
            }
            return ret;
        }

        // Helper method to convert WPF Color to hex string
        public static string convertColorToHex(Color color) => $"{color.R:X2}{color.G:X2}{color.B:X2}";

        public static Dictionary<string, string> getXmlElementContent(string xmlText)
        {
            // Load the XML into an XDocument
            XDocument doc = XDocument.Parse(xmlText);

            // Dictionary to store the element names and content
            Dictionary<string, string> elementContents = [];

            // Recursively add elements and their content to the dictionary
            foreach (XElement element in doc.Root.DescendantsAndSelf())
            {
                // Check if the element has content and add it to the dictionary
                if (!string.IsNullOrWhiteSpace(element.Value))
                {
                    elementContents[element.Name.LocalName] = element.Value;
                }
            }

            return elementContents;
        }

        public static Dictionary<string, string> replaceKeys(Dictionary<string, string> original, Dictionary<string, string> replacement)
        {
            var updatedOriginal = new Dictionary<string, string>();

            foreach (var kvp in original)
            {
                if (replacement.ContainsKey(kvp.Key))
                {
                    // Use the value from dict2 as the new key
                    string newKey = replacement[kvp.Key];
                    updatedOriginal[newKey] = kvp.Value;
                }
                else
                {
                    // If no replacement key is found in dict2, keep the original key
                    updatedOriginal[kvp.Key] = kvp.Value;
                }
            }

            return updatedOriginal;
        }

        public static bool isFileInUse(string filePath)
        {
            try
            {
                // Try to open the file with exclusive access
                using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                stream.Close(); // If no exception, file is not in use
            }
            catch (IOException)
            {
                // IOException will be thrown if the file is in use
                return true;
            }

            // If no exception, the file is not in use
            return false;
        }

        public static (List<string> chords, List<int> positions) extractChordsWithPositions(string inputString)
        {
            List<string> chords = [];
            List<int> positions = [];

            // Regex to match non-whitespace sequences in the input string
            Regex regex = new(@"\S+");
            MatchCollection matches = regex.Matches(inputString);

            // Loop through each match and capture the group and its position
            foreach (Match match in matches)
            {
                chords.Add(match.Value);        // Add the matched word (group) to the chords list
                positions.Add(match.Index);     // Add the starting index of the group to the positions list
            }

            return (chords, positions);  // Return both the chords and positions as a tuple
        }

        public static (List<string> chords, List<int> positions)? isChordLine(string inputString)
        {
            var res = CBasicConverter.extractChordsWithPositions(inputString);

            if (res.chords.Count == 0)
                return null;

            foreach (var group in res.chords)
            {
                if (!CScales.isValidChord(group))
                {
                    return null; // Return null if any group is not a valid chord
                }
            }
            return res; // Return chords and positions if all are valid chords
        }

        public void copyPropertiesFrom<TSource>(TSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "Source object cannot be null.");
            }

            Type sourceType = source.GetType();
            Type targetType = this.GetType();

            // Iterate over all properties in the source object
            foreach (var sourcePropInfo in sourceType.GetProperties())
            {
                // Find the matching property in the target (current) object
                PropertyInfo targetPropInfo = targetType.GetProperty(sourcePropInfo.Name);

                // Check if both properties exist, are of the same type, and the target property is writable
                if (targetPropInfo != null && targetPropInfo.CanWrite &&
                    targetPropInfo.PropertyType == sourcePropInfo.PropertyType)
                {
                    // Get the value from the source property
                    var sourceValue = sourcePropInfo.GetValue(source);

                    // Assign the value to the target property (current object)
                    targetPropInfo.SetValue(this, sourceValue);
                }
            }
        }

        public virtual string updateTags(string textIn)
        {
            return textIn;
        }

        public virtual string build(List<CChordSheetLine> chordSheetLines)
        {
            return "";
        }

        public virtual List<CChordSheetLine> analyze(string text)
        {
            return [];
        }
        public virtual List<CChordSheetLine> analyze(string[] lines)
        {
            return [];
        }

    public string getPropertyValueByName(string propertyName)
        {

            // Get the type of the object
            Type objType = GetType();

            // Get the PropertyInfo for the given property name
            PropertyInfo propInfo = objType.GetProperty(propertyName) ?? throw new ArgumentException($"The property '{propertyName}' was not found on object of type '{objType.Name}'.");

            // Get the value of the property from the object
            return (string) propInfo.GetValue(this);
        }

        public void setPropertyByName(string propertyName, string value)
        {
            var property = this.GetType().GetProperty(propertyName);

            if (property != null && property.CanWrite)
            {
                // If the property exists and can be written, set its value
                property.SetValue(this, value, null);
            }
        }




    }
}
