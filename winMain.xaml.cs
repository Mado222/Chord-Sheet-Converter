using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
        private FileFormatTypes SourceFileFormatType { get => GetSelectedEnumValue<FileFormatTypes>(gbSource) ?? FileFormatTypes.ChordPro; set => SetSelectedRadioButton(gbSource, value); }
        private FileFormatTypes TargetFileFormatType { get => GetSelectedEnumValue<FileFormatTypes>(gbTarget) ?? FileFormatTypes.ChordPro; set => SetSelectedRadioButton(gbTarget, value); }

        private readonly CAllConverters allConverters = new ();

        private MyPdfViewer? docViewerWindow = null;

        private readonly winBatchProcessing winBatchProcessing = new  ();

        private readonly TranspositionParameters transpositionParams;

        public MainWindow()
        {
            InitializeComponent();

            // Set DataContext to this MainWindow
            //DataContext = this; // If binding to a DataGrid in XAML

            cbSourceKey.Items.Clear();
            cbSourceKey.ItemsSource = chromaticScale;  // Equivalent to cbKey.Items.AddRange
            cbSourceScaleType.ItemsSource = Enum.GetValues(enumType: typeof(ScaleType));

            cbSourceMode.Items.Clear();
            cbSourceMode.ItemsSource = Enum.GetValues(enumType: typeof(ScaleMode));

            cbTartgetKey.Items.Clear();
            cbTartgetKey.ItemsSource = chromaticScale;  // Equivalent to cbKey.Items.AddRange
            cbTargetScaleType.ItemsSource = Enum.GetValues(enumType: typeof(ScaleType));

            cbTargetMode.Items.Clear();
            cbTargetMode.ItemsSource = Enum.GetValues(enumType: typeof(ScaleMode));

            cbTransposeSteps.Items.Clear();
            cbTransposeSteps.ItemsSource = Enumerable.Range(-11, 23).Reverse().ToList();
            cbTransposeSteps.SelectedItem = 0;
            tabControlTranspose.Visibility = Visibility.Collapsed;

            // Initialize TranspositionParameters and set it as DataContext
            transpositionParams = new TranspositionParameters();
            DataContext = transpositionParams;

            //ShowHideTranspose(Visibility.Collapsed);
            updateGUI();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CreateRadioButtonsFromEnum(gbSource, FileFormatTypes.ChordPro, RadioButton_CheckedChanged_Source, [FileFormatTypes.DOCX]);
            CreateRadioButtonsFromEnum(gbTarget, FileFormatTypes.DOCX, RadioButton_CheckedChanged_Target, [FileFormatTypes.DOCX]);
            SetTargetRB(FileFormatTypes.ChordPro);

            cbNashvilleKey.Items.Clear();
            cbNashvilleKey.ItemsSource = chromaticScale;  // Equivalent to cbKey.Items.AddRange
            cbNashvilleScaleType.ItemsSource = Enum.GetValues(enumType: typeof(ScaleType));
            cbNashvilleScaleType.SelectedIndex = 0;

            customSettings.LoadSettings();
            loadTemplatesToComboBox(cbTemplates, customSettings);
            updateGUI();

            winBatchProcessing.SavingPath = customSettings.DefaultOutputDirectory;
            winBatchProcessing.Visibility = Visibility.Collapsed;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            docViewerWindow?.Close();
            winBatchProcessing?.Close();
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

            RadioButton ? firstRadioButton = null; // Keep track of the first radio button

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
            //TargetFileFormatType = sender.GetType()
            if (SourceFileFormatType == FileFormatTypes.ChordPro && TargetFileFormatType == FileFormatTypes.ChordPro ||
                SourceFileFormatType == FileFormatTypes.OpenSong && TargetFileFormatType == FileFormatTypes.OpenSong)
            {
                return;
            }
            docViewerWindow?.Hide();
            updateGUI();
        }
        #endregion

        #region GUI_CallBacks

        private void btConvert_Click(object sender, RoutedEventArgs e)
        {
            (string txt, List<CChordSheetLine> chordSheetLines) = allConverters.Convert(SourceFileFormatType, TargetFileFormatType, txtIn.Text);
            txtOut.Text = txt;
            if (TargetFileFormatType == FileFormatTypes.DOCX && chordSheetLines.Count > 0)
            {
                buildDocx(chordSheetLines);
            }
        }

        string UndoCache = "";
        private void btTranspose_Click(object sender, RoutedEventArgs e)
        {
            UndoCache = txtIn.Text;
            btTransposeUndo.IsEnabled = true;
            if ((bool) cbTranspose!.IsChecked!)
            {
                if (tabControlTranspose.SelectedIndex == 0)
                {
                    txtIn.Text = allConverters.GetConverter(SourceFileFormatType).Transpose(txtIn.Text, null, (int) cbTransposeSteps.SelectedItem);
                }
                else
                {
                    txtIn.Text = allConverters.GetConverter(SourceFileFormatType).Transpose(txtIn.Text, transpositionParams);
                    cbSourceKey.SelectedItem = cbTartgetKey.SelectedItem;
                }
            }
            else if (cbNashville.IsChecked == true)
            {
                txtIn.Text = allConverters.GetConverter(SourceFileFormatType).ConverToNashville(txtIn.Text, (string)cbNashvilleKey.SelectedItem, (ScaleType)cbNashvilleScaleType.SelectedItem);
            }
        }
        private void btTransposeUndo_Click(object sender, RoutedEventArgs e)
        {
            txtIn.Text = UndoCache;
            btTransposeUndo.IsEnabled = false;
            UndoCache = "";
        }

        private void cbNashville_Checked(object sender, RoutedEventArgs e)
        {
            HandleCheckedChanged(true, cbNashvilleKey);
            cbTranspose.IsChecked = false;
        }

        private void cbNashvill_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleCheckedChanged(false, cbNashvilleKey);
        }

        private void cbTranspose_Checked(object sender, RoutedEventArgs e)
        {
            HandleCheckedChanged(true, cbSourceKey, cbTartgetKey);
            cbNashville.IsChecked = false;
        }

        private void cbTranspose_Unchecked(object sender, RoutedEventArgs e)
        {
            HandleCheckedChanged(false, cbSourceKey, cbTartgetKey);
        }


        private void HandleCheckedChanged(bool isChecked, ComboBox keyComboBox, ComboBox? targetKeyComboBox = null)
        {
            if (isChecked)
            {
                keyComboBox.ItemsSource = chromaticScale; // Populate the combobox with the chromatic scale

                // Extract chords from the input text
                string[] chords = allConverters.ExtractAllChords(SourceFileFormatType, CBasicConverter.StringToLines(txtIn.Text));
                string key = GuessKey(chords);

                if (key != null)
                {
                    keyComboBox.SelectedItem = key; // Select the guessed key
                    if (targetKeyComboBox != null)
                    {
                        targetKeyComboBox.SelectedItem = key; // Set the same key for the target combobox
                    }
                }
                else
                {
                    keyComboBox.SelectedIndex = 0; // Default to the first key if no guess
                    if (targetKeyComboBox != null)
                    {
                        targetKeyComboBox.SelectedIndex = 0; // Default for target combobox as well
                    }
                }
            }
            updateGUI();
        }

        private void btMoveTargetToSource_Click(object sender, RoutedEventArgs e)
        {
            //SourceFileFormatType = TargetFileFormatType;
            SetSelectedFileFormatType(gbSource, TargetFileFormatType);
            txtIn.Text = txtOut.Text;
            txtOut.Text = "";
        }

        private void btSettings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow(customSettings);
            if (settingsWindow.ShowDialog() == true)
            {
                customSettings.SaveSettings(); // Save the updated settings
                winBatchProcessing.SavingPath = customSettings.DefaultOutputDirectory;
            }
        }

        private void btLoadSource_Click(object sender, RoutedEventArgs e)
        {
            // Open a file dialog with specified extensions and load content into TextBox
            SourceFileLoaded = getFilePathLoading(SourceFileFormatType, "");

            // Open the dialog and check if the user selected a file
            if (SourceFileLoaded != "")
            {
                // Load the selected file content into the TextBox
                string fileContent = File.ReadAllText(SourceFileLoaded);
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

        public void buildDocx(List<CChordSheetLine> chordSheetLines, string outputPath = "", bool displayPdf = true)
        {
            string docxFilePath = outputPath;
            string fileName = allConverters.GetConverter(FileFormatTypes.DOCX).Title;
            
            if (string.IsNullOrEmpty(outputPath))
            {
                docxFilePath = customSettings.DefaultOutputDirectory;
            }
            docxFilePath += @"\" + fileName + ".docx";
            docxFilePath = docxFilePath.Replace(@"\\", @"\");
            
            if (File.Exists(docxFilePath) || String.IsNullOrEmpty(fileName))
                docxFilePath = getFilePathSaving(fileFormatType: TargetFileFormatType, docxFilePath);

            if (docxFilePath != "") //User pressed Cancel?
            {
                string templateFilePath = customSettings.DefaultTemplateDirectory + @"\" + cbTemplates.Text;
                templateFilePath = templateFilePath.Replace(@"\\", @"\");

                
                string ret = CDocxFormatter.ReplaceInTemplate(templateFilePath, docxFilePath, allConverters.GetConverter(FileFormatTypes.DOCX), chordSheetLines);
                if (ret == "")
                    DocxToPdf(docxFilePath, displayPdf);
                else
                {
                    txtOut.Text = ret;
                }
            }
        }

        private void DocxToPdf(string docxFilePath, bool displayPdf = true )
        {
            DocxToPdfConverter docxToXpsConverter = new();

            // Convert DOCX to PDF
            string pdfOutputPath = Path.ChangeExtension(docxFilePath, "pdf");
            docxToXpsConverter.ConvertDocxToPdf(docxFilePath, pdfOutputPath);

            if (displayPdf)
            {
                // Create Document Viewer Window
                docViewerWindow ??= new();

                // Load the pdf file
                docViewerWindow.DisplayPdf(pdfOutputPath);

                // Show the window
                docViewerWindow.Show();
            }
        }


        private static void DoEvents() => System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, new Action(static delegate { }));
        #endregion

        #region Loading_Saving



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
                    MessageBox.Show("Please select a valid OpenSong or XML file (either no extension or .xml).", "Invalid File", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                }
            }

            // Return null if no valid file was selected
            return "";
        }

        #endregion


        //private void ShowHideTranspose(Visibility v)
        //{
        //    tabItemTransposeSimple.Visibility = v;
        //    btTranspose.Visibility = v;
        //    btTransposeUndo.Visibility = v;

        //    // Temporary variable for the inverted visibility
        //    Visibility invertedVisibility = v == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        //    // Apply inverted visibility
        //    btConvert.Visibility = invertedVisibility;
        //    btMoveTargetToSource.Visibility = invertedVisibility;
        //    gbTarget.Visibility = invertedVisibility;
        //}
        //private void ShowHideNashville(Visibility v)
        //{
        //    GridNashville.Visibility = v;
        //    btTranspose.Visibility = v;
        //    btTransposeUndo.Visibility = v;

        //    Visibility invertedVisibility = v == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

        //    btConvert.Visibility = invertedVisibility;
        //    btMoveTargetToSource.Visibility = invertedVisibility;
        //    gbTarget.Visibility = invertedVisibility;
        //}

        //private void ShowHideTemplate(Visibility v)
        //{
        //    lblTemplate.Visibility = v;
        //    cbTemplates.Visibility = v;
        //}



        private void updateGUI()
        {
            tabControlTranspose.Visibility = Visibility.Collapsed;
            GridNashville.Visibility = Visibility.Collapsed;
            gbTarget.Visibility= Visibility.Collapsed;
            btConvert.Visibility= Visibility.Collapsed;
            spTransposeButtons.Visibility = Visibility.Collapsed;

            lblTemplate.Visibility = Visibility.Collapsed;
            cbTemplates.Visibility = Visibility.Collapsed;

            if (TargetFileFormatType == FileFormatTypes.DOCX)
            {
                lblTemplate.Visibility = Visibility.Visible;
                cbTemplates.Visibility = Visibility.Visible;
            }


            if ((bool) cbTranspose.IsChecked!)
            {
                tabControlTranspose.Visibility= Visibility.Visible;
                btTranspose.Content = "Transpose";
                spTransposeButtons.Visibility= Visibility.Visible;
            }
            else if 
                ((bool) cbNashville!.IsChecked!) 
            {
                GridNashville.Visibility = Visibility.Visible;
                btTranspose.Content = "Convert";
                spTransposeButtons.Visibility=Visibility.Visible;
            }
            else
            {
                btConvert.Visibility = Visibility.Visible;
                gbTarget.Visibility = Visibility.Visible;
            }



        }

    }
}

