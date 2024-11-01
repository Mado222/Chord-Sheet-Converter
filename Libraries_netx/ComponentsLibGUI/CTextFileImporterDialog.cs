using System.Text;

namespace ComponentsLib_GUI
{
    public class CTextFileImporterDialog
    {
        /// <summary>
        /// Imports string columns from a text file.
        /// </summary>
        /// <param name="pathToFile">Path to file. If null, OpenFileDialog will be shown, and the selected path is updated.</param>
        /// <param name="splitter">Character used to separate columns.</param>
        /// <param name="numHeaderRows">Number of header rows that will be ignored.</param>
        /// <param name="header">Content of the header rows.</param>
        /// <returns>
        /// List of rows and columns as string arrays.
        /// Returns null if function failed.
        /// </returns>
        public static List<string[]>? ImportTextFile(ref string pathToFile, string splitter, int numHeaderRows, ref string header)
        {
            // Initialize the result data structure.
            var importedData = new List<string[]>();

            // Use OpenFileDialog if path is null or empty.
            if (string.IsNullOrEmpty(pathToFile))
            {
                using OpenFileDialog openFileDialog = new();
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pathToFile = openFileDialog.FileName;
                }
                else
                {
                    return null; // Return null if file selection is canceled.
                }
            }

            // Check if the specified file exists before attempting to read.
            if (!File.Exists(pathToFile))
            {
                return null; // Return null if the file does not exist.
            }

            // Read and process the file.
            try
            {
                using StreamReader readFile = new(pathToFile);
                var headerContent = new StringBuilder(); // Local variable to build the header content.

                string? line;
                // Read header rows.
                for (int i = 0; i < numHeaderRows; i++)
                {
                    line = readFile.ReadLine();
                    headerContent.AppendLine(line ?? string.Empty); // Handle possible nulls.
                }

                // Update the header reference after reading.
                header = headerContent.ToString();

                // Read data rows.
                while ((line = readFile.ReadLine()) != null)
                {
                    string[] row = line.Split(splitter);
                    importedData.Add(row);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}"); // Log the error message.
                return null; // Return null in case of an exception.
            }

            return importedData;
        }
    }


}
