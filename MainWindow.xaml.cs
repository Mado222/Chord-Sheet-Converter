using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static ChordSheetConverter.CScales;
using static ChordSheetConverter.UltimateGuitarToChordpro;
using static ChordSheetConverter.CDocxFormatter;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace ChordSheetConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public partial class MainWindow : Window
    {
        private static readonly string[] line_separators = ["\r\n", "\n"];
        private FileFormatTypes FileFormatTypeSource;
        private FileFormatTypes FileFormatTypeTarget;

        // ObservableCollection to hold the list of songs
        public ObservableCollection<CFileItem> FileItems { get; set; }
        public MainWindow()
        {
            InitializeComponent();

            //Just samples
            FileItems =
        [
            new () { fileNamePath = @"C:\Temp\Song1.txt", processStatus = "Not processed" },
            new () { fileNamePath = @"C:\Temp\Song2.txt", processStatus = "Not processed" }
        ];

            // Set DataContext to this MainWindow
            this.DataContext = this; // If binding to a DataGrid in XAML
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dgvFiles.AllowDrop = true;

            CreateRadioButtonsFromEnum(gbSource, FileFormatTypeSource, RadioButton_CheckedChanged_Source);
            CreateRadioButtonsFromEnum(gbTarget, FileFormatTypeTarget, RadioButton_CheckedChanged_Target);

            cbKey.Items.Clear();
            cbKey.ItemsSource = chromaticScale;  // Equivalent to cbKey.Items.AddRange
            cbScaleType.ItemsSource = Enum.GetValues(enumType: typeof(ScaleType));
            cbScaleType.SelectedIndex = 0;
            cbKey.Visibility = Visibility.Hidden;
            cbScaleType.Visibility = Visibility.Hidden;
        }

        enum FileFormatTypes
        {
            UltimateGuitar,
            OpenSong,
            ChordPro,
            DOCX
        }

        // Dictionary mapping FileFormatTypes to their respective file extensions
        private static readonly Dictionary<FileFormatTypes, string[]> fileExtensions = new()
    {
        { FileFormatTypes.UltimateGuitar, new[] { ".txt" } },
        { FileFormatTypes.OpenSong, new[] { ".", ".xml" } },
        { FileFormatTypes.ChordPro, new[] { ".cho", ".chopro" } }
    };

        private void UpdateSourceTarge()
        {
            FileFormatTypes? ffts = GetSelectedFileFormatType(gbSource);
            if (ffts is not null)
            {
                FileFormatTypeSource = (FileFormatTypes)ffts;
            }

            ffts = GetSelectedFileFormatType(gbTarget);
            if (ffts is not null)
            {
                FileFormatTypeTarget = (FileFormatTypes)ffts;
            }
        }

        private static FileFormatTypes? GetSelectedFileFormatType(GroupBox groupBox)
        {
            // Use a recursive helper function to find RadioButtons
            foreach (var element in FindVisualChildren<RadioButton>(groupBox))
            {
                if (element.IsChecked == true)
                {
                    return (FileFormatTypes)element.Tag;
                }
            }
            return null;
        }

        // Helper method to recursively find children of a specific type (e.g., RadioButton)
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }


        private void rbOriginal_Checked(object sender, RoutedEventArgs e)
        {
            if (rbOriginal.IsChecked == true)
            {
                if (txtSavingPath is not null)
                    txtSavingPath.IsEnabled = false;
                if (pbSaveTo is not null)
                    pbSaveTo.IsEnabled = false;
            }
        }

        private void rbSelected_Checked(object sender, RoutedEventArgs e)
        {
            if (rbSelected.IsChecked == true)
            {
                txtSavingPath.IsEnabled = true;
                pbSaveTo.IsEnabled = true;

                if ((txtSavingPath.Text == "") || !Directory.Exists(txtSavingPath.Text))
                {
                    txtSavingPath.Text = GetSavePath(txtSavingPath.Text);
                }
            }
        }

        private static string GetSavePath(string startPath)
        {
            // Use Ookii Dialogs FolderBrowserDialog
            VistaFolderBrowserDialog fb = new();

            if (Directory.Exists(startPath))
                fb.SelectedPath = startPath;
            else
                fb.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Show the dialog
            fb.ShowDialog();

            return fb.SelectedPath;
        }

        private void pbSaveTo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            txtSavingPath.Text = GetSavePath(txtSavingPath.Text);
        }

        private void btConvert_Click(object sender, RoutedEventArgs e)
        {
            ConvertLefttoRight("Test");
        }

        private void ConvertLefttoRight(string songtitle)
        {
            if (txtIn.text is not null)
            {
                Dictionary<string, string> xmlContent = [];
                List<(string tag, string text)> fullLyrics = [];


                string[] lines = txtIn.text.Split(line_separators, StringSplitOptions.None);
                string? key = null;
                if ((bool)cbNashvilleActive.IsChecked)
                {
                    key = cbKey.Text;
                }
                string[] converted = [];

                switch (FileFormatTypeSource)
                {
                    case FileFormatTypes.UltimateGuitar:
                        switch (FileFormatTypeTarget)
                        {
                            case FileFormatTypes.UltimateGuitar:
                                txtOut.text = txtIn.text;
                                break;
                            case FileFormatTypes.OpenSong:
                                converted = ConvertUGToOpenSong(lines, songtitle, key);
                                txtOut.text = string.Join(Environment.NewLine, converted);
                                break;
                            case FileFormatTypes.ChordPro:
                                converted = ConvertUGToChordPro(lines, songtitle, key);
                                txtOut.text = string.Join(Environment.NewLine, converted);
                                break;
                            case FileFormatTypes.DOCX:

                                break;
                        }
                        break;
                    case FileFormatTypes.OpenSong:
                        switch (FileFormatTypeTarget)
                        {
                            case FileFormatTypes.UltimateGuitar:
                                break;
                            case FileFormatTypes.OpenSong:
                                txtOut.text = txtIn.text;
                                break;
                            case FileFormatTypes.ChordPro:
                                (xmlContent, string lyrics) = ConvertOpenSong.convertToChordPro(txtIn.text);
                                txtOut.text = lyrics;
                                break;
                            case FileFormatTypes.DOCX:
                                OpenSongToDOCX();
                                break;
                        }
                        break;
                    case FileFormatTypes.ChordPro:
                        switch (FileFormatTypeTarget)
                        {
                            case FileFormatTypes.UltimateGuitar:
                                break;
                            case FileFormatTypes.OpenSong:
                                ChordProToDOCX(txtIn.text);
                                break;
                            case FileFormatTypes.ChordPro:
                                txtOut.text = txtIn.text;
                                break;
                            case FileFormatTypes.DOCX:
                                break;
                        }
                        break;
                }
            }
        }

        private void OpenSongToDOCX()
        {
            Dictionary<string, string> xmlContent = [];
            List<(string tag, string text)> fullLyrics = [];

            (xmlContent, fullLyrics) = ConvertOpenSong.convertToDocx(txtIn.text);
            txtOut.text = string.Join(Environment.NewLine, fullLyrics.Select(tuple => $"{tuple.tag}: {tuple.text}"));
            string inP = @"d:\OneDrive\Daten\Visual Studio\SongConverterWPF_net8\Template1.docx";
            string outP = @"d:\OneDrive\Daten\Visual Studio\SongConverterWPF_net8\Out.docx";
            xmlContent.Remove("song");
            string ret = ReplaceInTemplate(inP, outP, xmlContent, fullLyrics);
            if (ret != "")
                txtOut.text = ret;

        }
        private void ChordProToDOCX (string text)
        {
            CFont ft = new(new FontFamily("Consolas"), 14.0, FontWeights.Normal, FontStyles.Normal, Colors.Black);
            var replacements = new Dictionary<string, string>
                                {
                                    { "{Title}", "Paradise" },
                                    { "{Composer}", "John Prine" }
                                };
            //replaceTagsInDocx(@"d:\OneDrive\Daten\Visual Studio\SongConverterWPF_net8\Template1.docx", replacements);

            txtOut.rtfText = CMusicSheetToRTF.ConvertToRTF(CBasicConverter.stringToLines(text));
        }

        #region RadioButtons_FileFormatTypes
        // Assuming you have a Panel or GroupBox named panelRadioButtons in your form
        public static void CreateRadioButtonsFromEnum<T>(GroupBox groupBox, T selectedValue, RoutedEventHandler onCheckedChanged) where T : Enum
        {
            // Create a StackPanel to hold the RadioButtons (WPF equivalent of Windows Forms control container)
            StackPanel stackPanel = new()
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(10)
            };

            // Clear the existing content in the GroupBox
            groupBox.Content = null;

            // Get all the values from the enum
            var enumValues = Enum.GetValues(typeof(T));

            foreach (var value in enumValues)
            {
                // Create a new RadioButton
                RadioButton radioButton = new()
                {
                    Content = value.ToString(),  // Set the content of the RadioButton to the enum value
                    Tag = value,  // Store the enum value in the Tag property for later use
                    IsChecked = value.Equals(selectedValue),  // Check if it matches the selected value
                    Margin = new Thickness(0, 5, 0, 5)  // Add margin for spacing between buttons
                };

                // Attach the event handler for Checked
                radioButton.Checked += onCheckedChanged;

                // Add the RadioButton to the StackPanel
                stackPanel.Children.Add(radioButton);
            }

            // Set the StackPanel as the content of the GroupBox
            groupBox.Content = stackPanel;
        }

        private static RadioButton? GetSelectedRadioButtonInGroupBox(GroupBox groupBox)
        {
            foreach (var element in LogicalTreeHelper.GetChildren(groupBox))
            {
                if (element is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    return radioButton;
                }
            }
            return null;
        }

        // Event handler that triggers when any radio button's checked state changes
        private void RadioButton_CheckedChanged_Source(object sender, EventArgs e)
        {
            UpdateSourceTarge();
        }

        private void RadioButton_CheckedChanged_Target(object sender, EventArgs e)
        {
            UpdateSourceTarge();
        }
        #endregion

        private void EnDisBatchConverting(bool EnDis)
        {
            dgvFiles.IsEnabled = EnDis;
            gbSaveTo.IsEnabled = EnDis;
            txtSavingPath.IsEnabled = EnDis;
            pbSaveTo.IsEnabled = EnDis;
            btBatchConvert.IsEnabled = EnDis;
            btClear.IsEnabled = EnDis;
        }

        private void cbNashvilleActive_Checked(object sender, RoutedEventArgs e)
        {
            EnDisBatchConverting(!(bool)cbNashvilleActive.IsChecked);

            Visibility vis = Visibility.Hidden;
            if ((bool)cbNashvilleActive.IsChecked)
            {
                vis = Visibility.Visible;
            }
            cbKey.Visibility = vis;
            cbScaleType.Visibility = vis;
            if ((bool)cbNashvilleActive.IsChecked)
            {
                cbKey.ItemsSource = CScales.chromaticScale;  // Equivalent to cbKey.Items.AddRange
                string? chord = GuessKey();
                if (chord != null)
                {
                    cbKey.SelectedItem = chord;
                }
                else { cbKey.SelectedIndex = 0; }
            }
        }

        private string? GuessKey()
        {
            List<string[]> chords = ExtractChords(txtIn.text.Split(line_separators, StringSplitOptions.None));
            if (chords.Count > 0)
                if (chords[^1].Length > 0)
                    return chords[^1][^1];
            return null;
        }

        public static List<string[]> ExtractChords(string[] inputLines)
        {
            List<string[]> ret = [];
            foreach (string line in inputLines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var chordInfo = UltimateGuitarToChordpro.DetectChordsOrText(line);
                    if (chordInfo != null)
                    {
                        ret.Add([.. chordInfo.Value.chords]);
                    }
                }
            }
            return ret;
        }

        private void dgvFiles_Drop(object sender, DragEventArgs e)
        {
            if (e is not null && e.Data is not null)
            {
                string[]? files = (string[]?)e.Data.GetData(DataFormats.FileDrop);
                if (files is not null && files.Length > 0)
                {
                    FileItems.Clear();
                    foreach (string file in files)
                    {
                        if (fileExtensions[FileFormatTypeSource].Contains(Path.GetExtension(file)))
                        {
                            FileItems.Add(new CFileItem(file, "Not processed"));
                        }
                    }
                    dgvFiles.ItemsSource = FileItems;
                }
            }
        }


        private void btBatchConvert_Click(object sender, EventArgs e)
        {
            foreach (CFileItem fi in FileItems)
            {
                try
                {
                    string text = File.ReadAllText(fi.fileNamePath);
                    string? SavingPath = Path.GetDirectoryName(fi.fileNamePath);

                    if (rbSelected.IsChecked == true)
                    {
                        SavingPath = txtSavingPath.Text; ;
                    }
                    if (Directory.Exists(SavingPath))
                    {
                        txtIn.text = text;
                        string songtitle = Path.GetFileNameWithoutExtension(fi.fileNamePath);
                        ConvertLefttoRight(songtitle);
                        File.WriteAllText(SavingPath + @"\" +
                            songtitle +
                            fileExtensions[FileFormatTypeTarget][0],
                            txtOut.text);
                        fi.processStatus = "OK";
                        dgvFiles.Items.Refresh();
                        DoEvents();
                    }
                    else
                    { fi.processStatus = "Failed: Saving path does nor exist"; }
                }
                catch (Exception ee)
                {
                    fi.processStatus = "Failed:" + ee.ToString();
                }
            }
        }
        private void DoEvents() => System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(delegate { }));

        private void btClear_Click(object sender, RoutedEventArgs e) => FileItems.Clear();
    }
}
