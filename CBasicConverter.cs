using System.IO;
using System.Windows.Media;
using System.Xml.Linq;

namespace ChordSheetConverter
{
    public class CBasicConverter
    {
        private static readonly string[] line_separators = ["\r\n", "\n"];

        public static string[] stringToLines(string text) => text.Split(line_separators, StringSplitOptions.None);


        // Helper method to convert WPF Color to hex string
        public static string ConvertColorToHex(Color color) => $"{color.R:X2}{color.G:X2}{color.B:X2}";

        public static Dictionary<string, string> GetXmlElementContent(string xmlText)
        {
            // Load the XML into an XDocument
            XDocument doc = XDocument.Parse(xmlText);

            // Dictionary to store the element names and content
            Dictionary<string, string> elementContents = new Dictionary<string, string>();

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

    }
}
