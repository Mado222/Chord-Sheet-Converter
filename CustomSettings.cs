using Newtonsoft.Json;
using System;
using System.IO;

namespace ChordSheetConverter
{
    public class CustomSettings
    {
        public string defaultTemplateDirectory { get; set; } = "";
        public string defaultOutputDirectory { get; set; } = "";

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

        // Load settings from the JSON file and apply them to this instance
        public void loadSettings()
        {
            if (File.Exists(_settingsFile))
            {
                var json = File.ReadAllText(_settingsFile);
                JsonConvert.PopulateObject(json, this);  // This will load values into the existing instance
            }
            else
            {
                // If no settings file exists, use default values
                setDefaults();
            }
        }

        // Save current settings (all properties) to the JSON file
        public void saveSettings()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(_settingsFile, json);
        }

        // Set default values for properties
        public void setDefaults()
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            defaultTemplateDirectory = Path.Combine(documentsPath, "ChordSheetConverter", "Templates");
            defaultOutputDirectory = Path.Combine(documentsPath, "ChordSheetConverter");

            // Ensure the directories exist
            if (!Directory.Exists(defaultTemplateDirectory))
            {
                Directory.CreateDirectory(defaultTemplateDirectory);
            }
            if (!Directory.Exists(defaultOutputDirectory))
            {
                Directory.CreateDirectory(defaultOutputDirectory);
            }

            // Save the default settings
            saveSettings();
        }
    }
}
