
namespace ComponentsLib
{
    public class CTextFileImporter
    {
        /// <summary>
        /// Imports string colums from a text file
        /// </summary>
        /// <param name="pathToFile">Path to file. If null OpenFileDialog will be shown and selected path is updated</param>
        /// <param name="splitter">Splitter char to seperate columns</param>
        /// <param name="numHeaderRows">Number of header rows that will be ignored</param>
        /// <param name="Header">Content of the header rows</param>
        /// <returns>
        /// Rows and columns
        /// null if function failed
        /// </returns>
        public static List<string[]> ImportTextFile(ref string pathToFile, string splitter, int numHeaderRows, ref string Header)
        {
            List<string[]> importedData = [];

            if (File.Exists(pathToFile))
            {
                //Arrays für 
                string? line;
                try
                {
                    StreamReader readFile = new(pathToFile);
                    //Remove Header
                    Header = "";
                    for (int i = 0; i < numHeaderRows; i++)
                    {
                        line = readFile.ReadLine();
                        Header += line + Environment.NewLine;
                    }

                    while ((line = readFile.ReadLine()) != null)
                    {
                        string[] row = line.Split(splitter.ToCharArray());
                        importedData.Add(row);
                    }
                }
                catch
                {
                    importedData = [];
                }
            }
            return importedData;
        }
    }

}
