using Newtonsoft.Json;
using System.IO;

namespace ChordSheetConverter
{
    public class CustomSettings
    {
        public string DefaultTemplateDirectory { get; set; } = "";
        public string DefaultOutputDirectory { get; set; } = "";

        private readonly string _settingsFile;

        // Constructor sets the file path to the Local AppData folder
        public CustomSettings()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ChordSheetConverter");
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            _settingsFile = Path.Combine(appDataPath, "settings.json");
        }

        public string GetFullPathFromTemplateFileName(string fileName)
        {
            string p = DefaultTemplateDirectory + @"\" + fileName;
            p=p.Replace(@"\\", @"\");
            return p;
        }

        // Load settings from the JSON file and apply them to this instance
        public void LoadSettings()
        {
            if (File.Exists(_settingsFile))
            {
                var json = File.ReadAllText(_settingsFile);
                JsonConvert.PopulateObject(json, this);  // This will load values into the existing instance
            }
            else
            {
                // If no settings file exists, use default values
                SetDefaults();
            }
        }

        // Save current settings (all properties) to the JSON file
        public void SaveSettings()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(_settingsFile, json);
        }

        // Set default values for properties
        public void SetDefaults()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            DefaultTemplateDirectory = Path.Combine(documentsPath, "ChordSheetConverter", "Templates");
            DefaultOutputDirectory = Path.Combine(documentsPath, "ChordSheetConverter");

            // Ensure the directories exist
            if (!Directory.Exists(DefaultTemplateDirectory))
            {
                Directory.CreateDirectory(DefaultTemplateDirectory);
            }
            if (!Directory.Exists(DefaultOutputDirectory))
            {
                Directory.CreateDirectory(DefaultOutputDirectory);
            }

            // Save the default settings
            SaveSettings();
        }

        public void GetSettings()
        {
            var settingsWindow = new SettingsWindow(this);
            if (settingsWindow.ShowDialog() == true)
            {
                SaveSettings(); // Save the updated settings
            }
        }
    }
}
