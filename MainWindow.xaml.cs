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
using System.Windows.Xps.Packaging;

namespace ChordSheetConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public partial class MainWindow : Window
    {
        private enum FileFormatTypes
        {
            UltimateGuitar,
            OpenSong,
            ChordPro,
            DOCX
        }

        private static readonly string[] line_separators = ["\r\n", "\n"];
        private FileFormatTypes FileFormatTypeSource;
        private FileFormatTypes FileFormatTypeTarget;
        CustomSettings customSettings = new();

        private string LoadedSourceFile = "";
        private string SavedTargetFile = "";

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

            CreateRadioButtonsFromEnum(gbSource, FileFormatTypeSource, RadioButton_CheckedChanged_Source, [FileFormatTypes.DOCX]);
            CreateRadioButtonsFromEnum(gbTarget, FileFormatTypeTarget, RadioButton_CheckedChanged_Target, [FileFormatTypes.UltimateGuitar]);

            cbKey.Items.Clear();
            cbKey.ItemsSource = chromaticScale;  // Equivalent to cbKey.Items.AddRange
            cbScaleType.ItemsSource = Enum.GetValues(enumType: typeof(ScaleType));
            cbScaleType.SelectedIndex = 0;
            cbKey.Visibility = Visibility.Hidden;
            cbScaleType.Visibility = Visibility.Hidden;

            customSettings.loadSettings();
            loadTemplatesToComboBox(cbTemplates, customSettings);
            ShowHideTemplate(Visibility.Collapsed);
            ShowHideNashville(Visibility.Collapsed);
        }

        // Dictionary mapping FileFormatTypes to their respective file extensions
        private static readonly Dictionary<FileFormatTypes, string[]> fileExtensions = new()
    {
        { FileFormatTypes.UltimateGuitar, new[] { ".txt" } },
        { FileFormatTypes.OpenSong, new[] { ".", ".xml" } },
        { FileFormatTypes.ChordPro, new[] { ".cho", ".chopro" } }
    };

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
            //ConvertLefttoRight("Test");
            DocxToPdfConverter docxToXpsConverter = new();
            string docxFilePath = "d:\\OneDrive\\Daten\\Visual Studio\\SongConverterWPF_net8\\Out.docx";
            string pdfOutputPath = "d:\\OneDrive\\Daten\\Visual Studio\\SongConverterWPF_net8\\Out.pdf";
            // Convert DOCX to XPS
            docxToXpsConverter.convertDocxToPdf(docxFilePath, pdfOutputPath);

            // Create and show the Document Viewer Window
            DocViewerWindow docViewerWindow = new();

            // Load the pdf file
            docViewerWindow.webBrowserPdf.Navigate(new Uri(pdfOutputPath));

            // Show the window
            docViewerWindow.Show();
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
        private void ChordProToDOCX(string text)
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
        public static void CreateRadioButtonsFromEnum<T>(GroupBox groupBox, T selectedValue, RoutedEventHandler onCheckedChanged, T[] excludedValues) where T : Enum
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

            RadioButton firstRadioButton = null; // Keep track of the first radio button

            foreach (var value in enumValues)
            {
                // Skip the value if it's in the excludedValues array
                if (excludedValues != null && Array.Exists(excludedValues, excluded => excluded.Equals(value)))
                {
                    continue; // Skip this value if it's in the exclusion list
                }

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

                // Keep track of the first RadioButton
                if (firstRadioButton == null)
                {
                    firstRadioButton = radioButton;
                }

                // Add the RadioButton to the StackPanel
                stackPanel.Children.Add(radioButton);
            }

            // Set the StackPanel as the content of the GroupBox
            groupBox.Content = stackPanel;

            // If no RadioButton is checked, check the first one
            if (firstRadioButton != null && !stackPanel.Children.OfType<RadioButton>().Any(rb => rb.IsChecked == true))
            {
                firstRadioButton.IsChecked = true;
            }
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
            FileFormatTypes? ffts = GetSelectedFileFormatType(gbSource);
            if (ffts is not null)
            {
                FileFormatTypeSource = (FileFormatTypes)ffts;
                CreateRadioButtonsFromEnum(gbTarget, FileFormatTypeTarget, RadioButton_CheckedChanged_Target, [(FileFormatTypes)ffts]);
            }
        }

        private void RadioButton_CheckedChanged_Target(object sender, EventArgs e)
        {

            FileFormatTypes? ffts = GetSelectedFileFormatType(gbTarget);
            if (ffts is not null)
            {
                FileFormatTypeTarget = (FileFormatTypes)ffts;
                if (ffts == FileFormatTypes.DOCX)
                {
                    ShowHideTemplate(Visibility.Visible);
                }
                else
                    ShowHideTemplate(Visibility.Collapsed);
            }
        }

        private void ShowHideTemplate(Visibility v)
        {
            lblTemplate.Visibility = v;
            cbTemplates.Visibility = v;
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
            cbNashvilleCheckedchanged(true);
        }

        private void cbNashvilleActive_Unchecked(object sender, RoutedEventArgs e)
        {
            cbNashvilleCheckedchanged(false);
        }

        private void cbNashvilleCheckedchanged(bool ischecked)
        {
            EnDisBatchConverting(ischecked);

            Visibility vis = Visibility.Collapsed;
            if (ischecked)
            {
                vis = Visibility.Visible;
                cbKey.ItemsSource = chromaticScale;  // Equivalent to cbKey.Items.AddRange
                string? chord = GuessKey();
                if (chord != null)
                {
                    cbKey.SelectedItem = chord;
                }
                else { cbKey.SelectedIndex = 0; }
            }
            ShowHideNashville(vis);
        }

        private void ShowHideNashville(Visibility v)
        {
            cbScaleType.Visibility = v;
            cbKey.Visibility = v;
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

        private void btCopyTargetToSource_Click(object sender, RoutedEventArgs e) => txtIn.text = txtOut.text;

        private void btCopySourceToTarget_Click(object sender, RoutedEventArgs e) => txtOut.text = txtIn.text;

        private void btLoadSource_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btSaveSource_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btLoadTarget_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btSaveTarget_Click(object sender, RoutedEventArgs e)
        {
        }

        public void loadTemplatesToComboBox(ComboBox comboBox, CustomSettings settings)
        {
            // Get the template directory from settings
            string templateDirectory = settings.defaultTemplateDirectory;

            // Ensure the template directory exists
            if (!Directory.Exists(templateDirectory))
            {
                Directory.CreateDirectory(templateDirectory);
            }

            // Get all .docx files in the template directory
            string[] templateFiles = Directory.GetFiles(templateDirectory, "*.docx");

            // If no templates found, copy default Template1.docx from the application directory
            if (templateFiles.Length == 0)
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string defaultTemplatePath = Path.Combine(appDirectory, "Template1.docx");
                string targetTemplatePath = Path.Combine(templateDirectory, "Template1.docx");

                // Copy the default template if it doesn't already exist
                if (File.Exists(defaultTemplatePath))
                {
                    File.Copy(defaultTemplatePath, targetTemplatePath, true);
                    templateFiles = new[] { targetTemplatePath };
                }
                else
                {
                    throw new FileNotFoundException("Default template 'Template1.docx' not found in the application directory.");
                }
            }

            // Clear the ComboBox before populating
            comboBox.Items.Clear();

            // Add each template file to the ComboBox (just the file names, without the path)
            foreach (var file in templateFiles)
            {
                comboBox.Items.Add(Path.GetFileName(file));
            }

            // Optionally set the first template as selected
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 0;
            }
        }
    }
}
