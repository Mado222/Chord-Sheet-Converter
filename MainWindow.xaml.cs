using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using static ChordSheetConverter.CScales;
using FileFormatTypes = ChordSheetConverter.CAllConverters.FileFormatTypes;

namespace ChordSheetConverter
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "<Pending>")]
    public partial class MainWindow : System.Windows.Window
    {

        private readonly CustomSettings customSettings = new();

        private string SourceFileLoaded = "";
        private string TargetFileSaved = "";

        // ObservableCollection to hold the list of songs
        public ObservableCollection<CFileItem> FileItems { get; set; }
        private FileFormatTypes SourceFileFormatType { get => GetSelectedEnumValue<FileFormatTypes>(gbSource) ?? FileFormatTypes.ChordPro; set => SetSelectedRadioButton(gbSource, value); }
        private FileFormatTypes TargetFileFormatType { get => GetSelectedEnumValue<FileFormatTypes>(gbTarget) ?? FileFormatTypes.ChordPro; set => SetSelectedRadioButton(gbTarget, value); }

        private readonly CAllConverters allConverters = new ();

        private MyPdfViewer? docViewerWindow = null;

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
            DataContext = this; // If binding to a DataGrid in XAML
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dgvFiles.AllowDrop = true;

            CreateRadioButtonsFromEnum(gbSource, FileFormatTypes.ChordPro, RadioButton_CheckedChanged_Source, [FileFormatTypes.DOCX]);
            CreateRadioButtonsFromEnum(gbTarget, FileFormatTypes.DOCX, RadioButton_CheckedChanged_Target, [FileFormatTypes.DOCX]);
            SetTargetRB(FileFormatTypes.ChordPro);
            
            for (int i = 11; i >= -11; i--)
            {
                cbTransposeSteps.Items.Add(i);
            }
            cbTranspose.IsEnabled = false;
            cbNashvilleActive.IsEnabled = false;

            cbKey.Items.Clear();
            cbKey.ItemsSource = chromaticScale;  // Equivalent to cbKey.Items.AddRange
            cbScaleType.ItemsSource = Enum.GetValues(enumType: typeof(ScaleType));
            cbScaleType.SelectedIndex = 0;
            //cbKey.Visibility = Visibility.Hidden;
            //cbScaleType.Visibility = Visibility.Hidden;

            customSettings.LoadSettings();
            loadTemplatesToComboBox(cbTemplates, customSettings);
            ShowHideTemplate(Visibility.Collapsed);
            ShowHideNashville(Visibility.Collapsed);
            ShowHideTranspose(Visibility.Collapsed);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            docViewerWindow?.Close();
        }

        #region RadioButtons_FileFormatTypes
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

        private static void SetSelectedFileFormatType(GroupBox groupBox, FileFormatTypes fileFormatType)
        {
            // Use a recursive helper function to find RadioButtons
            foreach (var element in FindVisualChildren<RadioButton>(groupBox))
            {
                if ((FileFormatTypes)element.Tag == fileFormatType)
                {
                    element.IsChecked = true;
                    break;
                }
            }
        }

        // Helper method to recursively find children of a specific type (e.g., RadioButton)
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T t)
                    {
                        yield return t;
                    }

                    if (child is not null)
                    {
                        foreach (T childOfChild in FindVisualChildren<T>(child))
                        {
                            yield return childOfChild;
                        }
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

            RadioButton? firstRadioButton = null; // Keep track of the first radio button

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
                firstRadioButton ??= radioButton;

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


        public static void SetSelectedRadioButton<T>(GroupBox groupBox, T selectedValue) where T : Enum
        {
            if (groupBox.Content is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children.OfType<RadioButton>())
                {
                    if (child.Tag.Equals(selectedValue))
                    {
                        child.IsChecked = true;
                        break;
                    }
                }
            }
        }

        public static T? GetSelectedEnumValue<T>(GroupBox groupBox) where T : struct, Enum
        {
            if (groupBox.Content is StackPanel stackPanel)
            {
                foreach (var child in stackPanel.Children.OfType<RadioButton>())
                {
                    if (child.IsChecked == true)
                    {
                        return (T)child.Tag;
                    }
                }
            }

            return null; // Return null if no radio button is selected
        }


        // Event handler that triggers when any radio button's checked state changes
        private void RadioButton_CheckedChanged_Source(object sender, EventArgs e)
        {
            FileFormatTypes? ffts = GetSelectedFileFormatType(gbSource);
            if (ffts is not null)
            {
                SetTargetRB((FileFormatTypes)ffts);
                txtIn.Text = "";
            }
        }

        private void SetTargetRB(FileFormatTypes ffts)
        {
            List<FileFormatTypes> excludedValues = [ffts];
            switch (ffts)
            {
                case FileFormatTypes.UltimateGuitar:
                            excludedValues.Add(FileFormatTypes.DOCX);
                    excludedValues.Add(FileFormatTypes.ChordPro);
                    break;
                case FileFormatTypes.OpenSong:
                    excludedValues.Add(FileFormatTypes.UltimateGuitar);
                    break;
                case FileFormatTypes.ChordPro:
                    excludedValues.Clear();
                    excludedValues.Add(FileFormatTypes.UltimateGuitar);
                    break;
            }
            CreateRadioButtonsFromEnum(gbTarget, FileFormatTypes.OpenSong, RadioButton_CheckedChanged_Target, [.. excludedValues]); //Target does not care
        }

        private void RadioButton_CheckedChanged_Target(object sender, EventArgs e)
        {
            FileFormatTypes? ffts = GetSelectedFileFormatType(gbTarget);
            if (ffts is not null)
            {
                if (ffts == FileFormatTypes.DOCX)
                {
                    ShowHideTemplate(Visibility.Visible);
                }
                else
                    ShowHideTemplate(Visibility.Collapsed);
            }
            if (SourceFileFormatType == FileFormatTypes.ChordPro && TargetFileFormatType == FileFormatTypes.ChordPro)
            {
                cbTranspose.IsEnabled = true;
                cbNashvilleActive.IsEnabled = true;
                return;
            }
            else
            {
                cbTranspose.IsEnabled = false;
                cbNashvilleActive.IsEnabled = false;
            }
            ShowHideTranspose(Visibility.Collapsed);
            docViewerWindow?.Hide();
        }
        #endregion

        #region GUI_CallBacks
        private void pbSaveTo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            txtSavingPath.Text = GetSavePath(txtSavingPath.Text);
        }

        private void btConvert_Click(object sender, RoutedEventArgs e)
        {
            if (cbTranspose.IsChecked == true && SourceFileFormatType == FileFormatTypes.ChordPro)
            {
                txtOut.Text = CChordPro.TransposeChordPro(txtIn.Text, Convert.ToInt16(cbTransposeSteps.Text));
            }
            else if (cbNashvilleActive.IsChecked == true)
            {
                txtOut.Text = CChordPro.ConvertChordProToNashville(txtIn.Text, cbKey.Text, (ScaleType)cbScaleType.SelectedItem);
            }
            else
            {
                (string txt, List<CChordSheetLine> chordSheetLines) = allConverters.Convert(SourceFileFormatType, TargetFileFormatType, txtIn.Text);
                txtOut.Text = txt;
                if (TargetFileFormatType == FileFormatTypes.DOCX && chordSheetLines.Count > 0)
                {
                    buildDocx(chordSheetLines);
                    //buildRtf(chordSheetLines);
                }
            }
        }

        private void cbNashvilleActive_Checked(object sender, RoutedEventArgs e)
        {
            cbNashvilleCheckedchanged(true);
        }

        private void cbNashvilleActive_Unchecked(object sender, RoutedEventArgs e)
        {
            cbNashvilleCheckedchanged(false);
        }

        private void cbTranspose_Checked(object sender, RoutedEventArgs e)
        {
            ShowHideTranspose(Visibility.Visible);
        }

        private void cbTranspose_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowHideTranspose(Visibility.Collapsed);
        }


        private void cbNashvilleCheckedchanged(bool ischecked)
        {
            ShowHideNashville(ischecked ? Visibility.Visible : Visibility.Collapsed);

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
            GridNashville.Visibility = v;
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
                        if (CAllConverters.FileExtensions[SourceFileFormatType].Contains(Path.GetExtension(file)))
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
                        txtIn.Text = text;
                        (txtOut.Text, _) = allConverters.Convert(SourceFileFormatType, TargetFileFormatType, text);
                        File.WriteAllText(SavingPath + @"\" +
                            Path.GetFileNameWithoutExtension(fi.fileNamePath) +
                            CAllConverters.FileExtensions[TargetFileFormatType][0],
                            txtOut.Text);
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
        private void btClear_Click(object sender, RoutedEventArgs e) => FileItems.Clear();

        private void btMoveTargetToSource_Click(object sender, RoutedEventArgs e)
        {
            SetSelectedFileFormatType(gbSource, SourceFileFormatType);
            txtIn.Text = txtOut.Text;
            txtOut.Text = "";
        }

        private void btCopySourceToTarget_Click(object sender, RoutedEventArgs e) => txtOut.Text = txtIn.Text;

        

        private void btLoadSource_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog with specified extensions and load content into TextBox
            string ret = getFilePathLoading(SourceFileFormatType, "");

            // Open the dialog and check if the user selected a file
            if (ret != "")
            {
                // Load the selected file content into the TextBox
                string fileContent = File.ReadAllText(ret);
                fileContent = fileContent.Replace("<lyrics>", "<lyrics>" + Environment.NewLine);
                fileContent = fileContent.Replace("</lyrics>", Environment.NewLine + "</lyrics>");
                string[] lines = CBasicConverter.StringToLines(fileContent);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith(' '))
                    {
                        // Replace the first space with a non-breaking space
                        lines[i] = string.Concat(CChordSheetLine.nonBreakingSpace, lines[i].AsSpan(1));
                    }
                }
                txtIn.Text = CBasicConverter.LinesToString(lines);  // Load the file content as plain text
            }

            txtOut.Text = "";
            allConverters.ReplaceConverterWithNewObject(SourceFileFormatType);  //Start from scratch with this object
            //loadedChordSheetLines = allConverters.analyze(SourceFileFormatType, lines);
        }

        private void btAddInfo_Click(object sender, RoutedEventArgs e)
        {
            txtIn.Text = allConverters.UpdateTags(SourceFileFormatType, txtIn.Text);
        }

        private void btSaveSource_Click(object sender, RoutedEventArgs e)
        {
            SourceFileLoaded = SaveFileFromTextBox(SourceFileFormatType, txtIn, SourceFileLoaded);
        }

        private void btLoadTarget_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btSaveTarget_Click(object sender, RoutedEventArgs e)
        {
            if (CAllConverters.FileExtensions.TryGetValue(TargetFileFormatType, out string[]? value))
            {
                string[] extensions = value;
                TargetFileSaved = Path.ChangeExtension(SourceFileLoaded, extensions[0]);
            }
            TargetFileSaved = SaveFileFromTextBox(TargetFileFormatType, txtOut, TargetFileSaved);
        }
        #endregion

        #region HelperFunctions

        public void buildDocx(List<CChordSheetLine> chordSheetLines)
        {
            string fileName = allConverters.GetConverter(FileFormatTypes.DOCX).Title;
            string docxFilePath = customSettings.DefaultOutputDirectory + @"\" + fileName + ".docx";
            docxFilePath = docxFilePath.Replace(@"\\", @"\");
            docxFilePath = getFilePathSaving(fileFormatType: TargetFileFormatType, docxFilePath);

            if (docxFilePath != "") //User pressed Cancel?
            {
                string templateFilePath = customSettings.DefaultTemplateDirectory + @"\" + cbTemplates.Text;
                templateFilePath = templateFilePath.Replace(@"\\", @"\");

                string ret = CDocxFormatter.ReplaceInTemplate(templateFilePath, docxFilePath, allConverters.GetConverter(FileFormatTypes.DOCX), chordSheetLines);
                if (ret == "")
                    DocxToPdf(docxFilePath);
                else
                {
                    txtOut.Text = ret;
                }
            }
        }


        private void DocxToPdf(string docxFilePath)
        {
            DocxToPdfConverter docxToXpsConverter = new();

            // Convert DOCX to PDF
            string pdfOutputPath = Path.ChangeExtension(docxFilePath, "pdf");
            docxToXpsConverter.ConvertDocxToPdf(docxFilePath, pdfOutputPath);

            // Create Document Viewer Window
            docViewerWindow ??= new();

            // Load the pdf file
            docViewerWindow.DisplayPdf(pdfOutputPath);

            // Show the window
            docViewerWindow.Show();
        }

        private string? GuessKey()
        {
            /*
            List<string[]> chords = ExtractChords(txtIn.Text.Split(line_separators, StringSplitOptions.None));
            if (chords.Count > 0)
                if (chords[^1].Length > 0)
                    return chords[^1][^1];*/
            return null;
        }

        /*
        public static List<string[]> ExtractChords(string[] inputLines)
        {
            List<string[]> ret = [];
            foreach (string line in inputLines)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    var chordInfo = CUltimateGuitarConverter.isChordLine(line);
                    if (chordInfo != null)
                    {
                        ret.Add([.. chordInfo.Value.chords]);
                    }
                }
            }
            return ret;
        }*/

        private void EnDisBatchConverting(bool EnDis)
        {
            dgvFiles.IsEnabled = EnDis;
            gbSaveTo.IsEnabled = EnDis;
            txtSavingPath.IsEnabled = EnDis;
            pbSaveTo.IsEnabled = EnDis;
            btBatchConvert.IsEnabled = EnDis;
            btClear.IsEnabled = EnDis;
        }

        /*
        private void EnDisTranspose(bool EnDis)
        {
            gbTranspose.IsAncestorOf(this);
            cbTranspose.IsEnabled = EnDis;
            cbTransposeSteps.IsEnabled = EnDis;
            lblTranspose.IsEnabled = EnDis;
        }*/

        private void ShowHideTemplate(Visibility v)
        {
            lblTemplate.Visibility = v;
            cbTemplates.Visibility = v;
        }

        private void ShowHideTranspose(Visibility v)
        {
            GridTranspose.Visibility = v;
        }


        private static void DoEvents() => System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(static delegate { }));
        #endregion

        #region Loading_Saving

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
        public static void loadTemplatesToComboBox(ComboBox comboBox, CustomSettings settings)
        {
            // Get the template directory from settings
            string templateDirectory = settings.DefaultTemplateDirectory;

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
                    templateFiles = [targetTemplatePath];
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

        private static string getFilePathSaving(FileFormatTypes fileFormatType, string defaultFilePath = "")
        {
            // Create the SaveFileDialog
            SaveFileDialog saveFileDialog = new()
            {
                OverwritePrompt = true, // Let the system handle the overwrite prompt
                Filter = GenerateFileDialogFilter(fileFormatType)
            };

            // Set the initial directory and file name if provided
            if (!string.IsNullOrEmpty(defaultFilePath))
            {
                string directory = Path.GetDirectoryName(defaultFilePath);  // Get directory
                string fileName = Path.GetFileName(defaultFilePath);        // Get just the file name

                if (!string.IsNullOrEmpty(directory))
                {
                    saveFileDialog.InitialDirectory = directory;  // Set initial directory
                }

                if (!string.IsNullOrEmpty(fileName))
                {
                    saveFileDialog.FileName = fileName;  // Set file name
                }
            }

            // Open the save file dialog and check if the user selected a file
            if (saveFileDialog.ShowDialog() == true)
            {
                // Return the file path used for saving
                return saveFileDialog.FileName;
            }

            // If the user cancels or no file is selected, return an empty string
            return "";
        }


        private static string getFilePathLoading(FileFormatTypes fileFormatType, string startPath)
        {
            string ret = "";
            // Create the OpenFileDialog
            OpenFileDialog openFileDialog = new()
            {
                // Set the filter based on the file format type
                Filter = GenerateFileDialogFilter(fileFormatType),
                FileName = startPath
            };
            if (openFileDialog.ShowDialog() == true)
            {
                // Load the selected file content into the TextBox
                ret = openFileDialog.FileName;
            }
            return ret;
        }



        // Helper method to generate the filter string for OpenFileDialog based on FileFormatTypes
        private static string GenerateFileDialogFilter(FileFormatTypes fileFormatType)
        {
            if (CAllConverters.FileExtensions.TryGetValue(fileFormatType, out string[]? value))
            {
                string[] extensions = value;

                // Create a filter string in the format: "FileType (*.ext1;*.ext2)|*.ext1;*.ext2"
                string filter = $"{fileFormatType} Files ({string.Join(";", Array.ConvertAll(extensions, ext => $"*{ext}"))})|{string.Join(";", Array.ConvertAll(extensions, ext => $"*{ext}"))}";

                return filter;
            }

            return "All Files (*.*)|*.*"; // Default filter if no matching extensions are found
        }


        // Method to save the contents of the TextBox to a file, using the system's overwrite prompt, with a default file path
        private static string SaveFileFromTextBox(FileFormatTypes fileFormatType, TextBox txtIn, string defaultFilePath = "")
        {
            string path = getFilePathSaving(fileFormatType, defaultFilePath);

            // Write the content of the TextBox to the selected file
            if (path != "")
                File.WriteAllText(path, txtIn.Text);
            return path;
        }
            
        


        // Method to open a file dialog and manually filter files with or without an extension, and return the chosen file path
        public static string LoadOpenSongFileToTextBox(TextBox txtIn)
        {
            // Create the OpenFileDialog
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Files without extension|*|All files (*)|*.*", //"OpenSong and XML Files (*.xml;*)|*.xml;*", // Allow .xml and all files
                Multiselect = true  // For single file selection
            };

            // Open the dialog and check if the user selected a file
            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFilePath = openFileDialog.FileName;

                // Filter: Include files with no extension or with ".xml" extension
                string fileExtension = Path.GetExtension(selectedFilePath);
                if (string.IsNullOrEmpty(fileExtension) || fileExtension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    // Load the selected file content into the TextBox
                    string fileContent = File.ReadAllText(selectedFilePath);
                    txtIn.Text = fileContent;

                    // Return the file path of the selected file
                    return selectedFilePath;
                }
                else
                {
                    // Handle the case where the file has an invalid extension
                    System.Windows.MessageBox.Show("Please select a valid OpenSong or XML file (either no extension or .xml).", "Invalid File", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }

            // Return null if no valid file was selected
            return "";
        }

        #endregion

        private void btSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(customSettings);
            if (settingsWindow.ShowDialog() == true)
            {
                customSettings.SaveSettings(); // Save the updated settings
            }
        }
    }
}

