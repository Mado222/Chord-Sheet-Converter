using System.Windows;
using Microsoft.Win32;
using System.Windows.Forms;

namespace ChordSheetConverter
{
    public partial class SettingsWindow : Window
    {
        public CustomSettings Settings { get; }

        public SettingsWindow(CustomSettings settings)
        {
            InitializeComponent();
            Settings = settings;
            DataContext = Settings;
        }

        private void BrowseTemplateDirectory_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = Settings.DefaultTemplateDirectory;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Settings.DefaultTemplateDirectory = dialog.SelectedPath;
                }
            }
        }

        private void BrowseOutputDirectory_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = Settings.DefaultOutputDirectory;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    Settings.DefaultOutputDirectory = dialog.SelectedPath;
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
