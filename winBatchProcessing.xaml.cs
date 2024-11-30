using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;
using static ChordSheetConverter.CAllConverters;
using static ChordSheetConverter.CScales;

namespace ChordSheetConverter
{
    /// <summary>
    /// Interaction logic for winBatchProcessing.xaml
    /// </summary>
    public partial class winBatchProcessing : Window
    {
        private FileFormatTypes SourceFileFormatType { get; set; } = FileFormatTypes.ChordPro;
        private FileFormatTypes TargetFileFormatType { get; set; } = FileFormatTypes.ChordPro;


        public ObservableCollection<CFileItem> FileItems { get; set; }
        private string _savingPath = "Path";

        public string SavingPath
        {
            get => _savingPath;
            set
            {
                if (_savingPath != value)
                {
                    _savingPath = value;
                    OnPropertyChanged(nameof(SavingPath));
                }
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public winBatchProcessing()
        {
            InitializeComponent();

            //Just samples
            FileItems =
        [
            new () { fileNamePath = @"Drag / Drop here", processStatus = "" }
        ];

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dgvFiles.AllowDrop = true;
        }

        private void pbSaveTo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            txtSavingPath.Text = GetSavePath(txtSavingPath.Text);
        }


        private void btBatchConvert_Click(object sender, EventArgs e)
        {
            StartBatchProcess();
        }

        private void EnDisBatchConverting(bool EnDis)
        {
            dgvFiles.IsEnabled = EnDis;
            gbSaveTo.IsEnabled = EnDis;
            txtSavingPath.IsEnabled = EnDis;
            pbSaveTo.IsEnabled = EnDis;
            btBatchConvert.IsEnabled = EnDis;
            btClear.IsEnabled = EnDis;
        }

        private async void StartBatchProcess()
        {
            pbBatchConvert.Maximum = FileItems.Count;
            pbBatchConvert.Minimum = 0;
            pbBatchConvert.Width = 300;
            pbBatchConvert.Visibility = Visibility.Visible;

            //// Create a progress reporter to handle updates to the progress bar
            //var progress = new Progress<int>(value =>
            //{
            //    pbBatchConvert.Value = value;
            //});

            //await Task.Run(() =>
            //{
            //    int i = 0;

            //    foreach (CFileItem fi in FileItems)
            //    {
            //        try
            //        {
            //            string text = File.ReadAllText(fi.fileNamePath);
            //            string? SavingPath = Path.GetDirectoryName(fi.fileNamePath);

            //            Dispatcher.Invoke(() =>
            //            {
            //                if (rbSelected.IsChecked == true)
            //                {
            //                    SavingPath = txtSavingPath.Text;
            //                }
            //            });

            //            if (Directory.Exists(SavingPath))
            //            {
            //                // Update text input from file
            //                Dispatcher.Invoke(() => txtIn.Text = text);

            //                // Handle conversion based on settings in the UI
            //                Dispatcher.Invoke(() =>
            //                {
            //                    if (cbTranspose.IsChecked == true && SourceFileFormatType == FileFormatTypes.ChordPro)
            //                    {
            //                        txtOut.Text = CChordPro.TransposeChordPro(txtIn.Text, Convert.ToInt16(cbTransposeSteps.Text));
            //                    }
            //                    else if (cbNashvilleActive.IsChecked == true)
            //                    {
            //                        txtOut.Text = CChordPro.ConvertChordProToNashville(txtIn.Text, cbKey.Text, (ScaleType)cbScaleType.SelectedItem);
            //                    }
            //                    else
            //                    {
            //                        (string txt, List<CChordSheetLine> chordSheetLines) = allConverters.Convert(SourceFileFormatType, TargetFileFormatType, txtIn.Text);
            //                        txtOut.Text = txt;

            //                        if (TargetFileFormatType == FileFormatTypes.DOCX && chordSheetLines.Count > 0)
            //                        {
            //                            buildDocx(chordSheetLines, SavingPath + @"\", displayPdf: false);
            //                        }
            //                        else
            //                        {
            //                            File.WriteAllText(SavingPath + @"\" +
            //                                Path.GetFileNameWithoutExtension(fi.fileNamePath) +
            //                                CAllConverters.FileExtensions[TargetFileFormatType][0],
            //                                txtOut.Text);
            //                        }
            //                    }
            //                });

            //                Dispatcher.Invoke(() =>
            //                {
            //                    fi.processStatus = "OK";
            //                    dgvFiles.Items.Refresh();
            //                });
            //            }
            //            else
            //            {
            //                Dispatcher.Invoke(() => fi.processStatus = "Failed: Saving path does not exist");
            //            }
            //        }
            //        catch (Exception ee)
            //        {
            //            Dispatcher.Invoke(() => fi.processStatus = "Failed: " + ee.ToString());
            //        }

            //        i++;
            //        ((IProgress<int>)progress).Report(i); // Report progress here
            //        DoEvents();
            //    }
            //});

            pbBatchConvert.Value = 0;
            pbBatchConvert.Visibility = Visibility.Collapsed;
        }


        private void btClear_Click(object sender, RoutedEventArgs e) => FileItems.Clear();

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
                        if ((SourceFileFormatType == FileFormatTypes.OpenSong &&
                            string.IsNullOrEmpty(Path.GetExtension(file))) ||
                            CAllConverters.FileExtensions[SourceFileFormatType].Contains(Path.GetExtension(file)))
                        {
                            FileItems.Add(new CFileItem(file, "Not processed"));
                        }
                        dgvFiles.ItemsSource = FileItems;
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
                if (spSavePath is not null)
                    spSavePath.Visibility = Visibility.Collapsed;
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
                if (spSavePath is not null)
                    spSavePath.Visibility = Visibility.Visible;
            }
        }

        private string GetSavePath(string startPath)
        {
            using (var dialog = new System.Windows.Forms.OpenFileDialog())
            {
                dialog.InitialDirectory = Directory.Exists(startPath) ? startPath : Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dialog.Filter = "Folder|*.none"; // Set a filter that doesn’t match any real files
                dialog.CheckFileExists = false;
                dialog.CheckPathExists = true;
                dialog.FileName = "Select Folder"; // Text shown in the File name field

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    // Use the directory path from the selected "file"
                    return System.IO.Path.GetDirectoryName(dialog.FileName) ?? startPath;
                }
            }

            return startPath; // Return the original path if the dialog was canceled
        }


    }
}
