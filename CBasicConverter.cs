using System.DirectoryServices;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Xml.Linq;
using static ChordSheetConverter.CAllConverters;
using static ChordSheetConverter.CScales;

namespace ChordSheetConverter
{
    /// <summary>
    ///   Basic class for all converters
    /// </summary>
    public partial class CBasicConverter : IChordSheetAnalyzer
    {
        //Common properties
        public string Title { get; set; } = "";
        public string Composer { get; set; } = "";
        public string Copyright { get; set; } = "";
        public string Capo { get; set; } = "";
        public string Tempo { get; set; } = "";
        public string Time { get; set; } = "";
        public string Key { get; set; } = "";

        // Properties for ChordPro
        //public string sortTitle { get; set; } = "";
        public string SubTitle { get; set; } = "";
        public string Artist { get; set; } = "";
        public string Lyricist { get; set; } = "";
        public string Album { get; set; } = "";
        public string Year { get; set; } = "";
        public string Duration { get; set; } = "";

        public void FillPropertiesWithDefaults()
        {
            if (string.IsNullOrEmpty(Title)) Title = "Untitled";
            if (string.IsNullOrEmpty(Composer)) Composer = "Unknown Composer";
            if (string.IsNullOrEmpty(Copyright)) Copyright = "No Copyright Info";
            if (string.IsNullOrEmpty(Capo)) Capo = "None";
            if (string.IsNullOrEmpty(Tempo)) Tempo = "120";  // Default tempo
            if (string.IsNullOrEmpty(Time)) Time = "4/4";  // Default time signature
            if (string.IsNullOrEmpty(Key)) Key = "C";  // Default key

            // For ChordPro properties
            if (string.IsNullOrEmpty(SubTitle)) SubTitle = "No Subtitle";
            if (string.IsNullOrEmpty(Artist)) Artist = "Unknown Artist";
            if (string.IsNullOrEmpty(Lyricist)) Lyricist = "Unknown Lyricist";
            if (string.IsNullOrEmpty(Album)) Album = "Unknown Album";
            if (string.IsNullOrEmpty(Year)) Year = "Unknown Year";
            if (string.IsNullOrEmpty(Duration)) Duration = "Unknown Duration";
        }


        public static readonly Dictionary<string, string> _propertyMapDisplayNames = new()
        {
    { "Title", "Song Title" },
    { "Author", "Author Name" },
    { "SubTitle", "Subtitle" },
    { "Composer", "Composer" },
    { "Lyricist", "Lyricist" },
    { "Copyright", "Copyright" },
    { "Year", "Year (Released, Written ...)" },
    { "Key", "Key" },
    { "Time", "Time Signature" },
    { "Tempo", "Tempo" },
    { "Capo", "Capo" }
        };

        public Dictionary<string, string> PropertyMapDisplayNames { get; } = _propertyMapDisplayNames;

        public virtual Dictionary<string, string> PropertyMapTags => throw new NotImplementedException();

        private static List<CChordSheetLine> _chordSheetLines = [];

        public static List<CChordSheetLine> ChordSheetLines
        {
            get { return _chordSheetLines; }
            set { _chordSheetLines = value ?? []; }  // Null-check
        }


        public static string[] StringToLines(string text) => text.Split(CChordSheetLine.line_separators, StringSplitOptions.None);
        public static string LinesToString(string[] lines) => string.Join(Environment.NewLine, lines);

        public static string GetLines(List<CChordSheetLine> csLines)
        {
            string ret = "";
            foreach (CChordSheetLine line in csLines)
            {
                ret += line.Line + CChordSheetLine.line_separators[0];
            }
            return ret;
        }

        // Helper method to convert WPF Color to hex string
        public static string ConvertColorToHex(Color color) => $"{color.R:X2}{color.G:X2}{color.B:X2}";

        public static Dictionary<string, string> GetXmlElementContent(string xmlText)
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

        public static Dictionary<string, string> ReplaceKeys(Dictionary<string, string> original, Dictionary<string, string> replacement)
        {
            var updatedOriginal = new Dictionary<string, string>();

            foreach (var kvp in original)
            {
                if (replacement.TryGetValue(kvp.Key, out string? value))
                {
                    // Use the value from dict2 as the new key
                    string newKey = value;
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

        public static bool IsFileInUse(string filePath)
        {
            try
            {
                // Try to open the file with exclusive access
                using FileStream stream = new(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
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

        [GeneratedRegex(@"\S+")]
        private static partial Regex RegExExtractChordsWithPositions();

        public static (List<string> chords, List<int> positions) ExtractChordsWithPositions(string inputString)
        {
            List<string> chords = [];
            List<int> positions = [];

            // Regex to match non-whitespace sequences in the input string
            Regex regex = RegExExtractChordsWithPositions();
            MatchCollection matches = regex.Matches(inputString);

            // Loop through each match and capture the group and its position
            foreach (Match match in matches)
            {
                chords.Add(match.Value);        // Add the matched word (group) to the chords list
                positions.Add(match.Index);     // Add the starting index of the group to the positions list
            }

            return (chords, positions);  // Return both the chords and positions as a tuple
        }

        public static (List<string> chords, List<int> positions)? IsChordLine(string inputString)
        {
            var res = CBasicConverter.ExtractChordsWithPositions(inputString);

            if (res.chords.Count == 0)
                return null;

            foreach (var group in res.chords)
            {
                if (!CScales.IsValidChord(group))
                {
                    return null; // Return null if any group is not a valid chord
                }
            }
            return res; // Return chords and positions if all are valid chords
        }

        public void CopyPropertiesFrom<TSource>(TSource source)
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

        public virtual string UpdateTags(string textIn)
        {
            return textIn;
        }

        public virtual string Build(List<CChordSheetLine> chordSheetLines)
        {
            return "";
        }

        public virtual List<CChordSheetLine> Analyze(string text)
        {
            return [];
        }
        public virtual List<CChordSheetLine> Analyze(string[] lines)
        {
            return [];
        }

        public string GetPropertyValueByName(string propertyName)
        {

            // Get the type of the object
            Type objType = GetType();

            // Get the PropertyInfo for the given property name
            PropertyInfo propInfo = objType.GetProperty(propertyName) ?? throw new ArgumentException($"The property '{propertyName}' was not found on object of type '{objType.Name}'.");

            // Get the value of the property from the object
            return (string)propInfo.GetValue(this);
        }

        public void SetPropertyByName(string propertyName, string value)
        {
            var property = this.GetType().GetProperty(propertyName);

            if (property != null && property.CanWrite)
            {
                // If the property exists and can be written, set its value
                property.SetValue(this, value, null);
            }
        }

        protected static (string tagName, string tagValue) GetTags(string line)
        {
            if (line.StartsWith('{') && line.EndsWith('}'))
            {
                line = line.Trim('{', '}');
                var parts = line.Split(':');
                string tagName = parts[0].Trim();
                string tagValue = "";

                if (parts.Length > 1)
                {
                    tagValue = parts[1].Trim();
                    if (tagValue.Contains("label="))
                    {
                        int labelIndex = tagValue.IndexOf("label=") + 6;
                        tagValue = tagValue[labelIndex..].Trim('"');
                    }
                }

                return (tagName, tagValue);
            }

            return ("", "");
        }

        public virtual string Transpose(string textIn, TranspositionParameters? parameters = null, int? steps = null)
        {
            return LinesToString(Transpose(StringToLines(textIn), parameters, steps));
        }

        public virtual string[] Transpose(string[] linesIn, TranspositionParameters? parameters = null, int? steps = null)
        {
            return Array.Empty<string>();
        }

        public virtual (CChordCollection chords, string lyrics) ExtractChords(string line)
        {
            return (new CChordCollection(), "");
        }

        public virtual string ConverToNashville(string text, string key, ScaleType scaleType = ScaleType.Major)
        {
            return "";
        }
    }
}
