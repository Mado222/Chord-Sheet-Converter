using System;
using System.IO;
using System.Windows;

namespace ChordSheetConverter
{
    public class MyPdfViewer : DocViewerWindow
    {
        private string _currentTempPdfPath;

        public MyPdfViewer()
        {
            // Initialize the window and make sure we start with no temporary file
            _currentTempPdfPath = string.Empty;
        }

        /// <summary>
        /// Displays a PDF in the viewer by copying it to a temporary location.
        /// Any previous temp file is deleted, and this file is deleted on close.
        /// </summary>
        /// <param name="pdfFilePath">Path to the PDF file to display.</param>
        public async void DisplayPdf(string pdfFilePath = "")
        {
            try
            {
                // Navigate to a blank page to release any lock on the previous PDF file
                webBrowserPdf.Navigate("about:blank");

                // Introduce a short delay to ensure the WebBrowser has time to release the file lock
                await Task.Delay(500);

                // Delete any existing temporary file
                DeleteTempPdf();

                if (pdfFilePath != "")
                {
                    // Create a new unique temporary path
                    _currentTempPdfPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");

                    // Copy the PDF to the temp file
                    File.Copy(pdfFilePath, _currentTempPdfPath, overwrite: true);

                    // Display the PDF in the WebBrowser control
                    webBrowserPdf.Navigate(new Uri("file:///" + _currentTempPdfPath));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error displaying PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        /// <summary>
        /// Deletes the current temporary PDF file if it exists.
        /// </summary>
        private void DeleteTempPdf()
        {
            if (!string.IsNullOrEmpty(_currentTempPdfPath) && File.Exists(_currentTempPdfPath))
            {
                try
                {
                    File.Delete(_currentTempPdfPath);
                    _currentTempPdfPath = string.Empty;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not delete temp PDF file: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        /// <summary>
        /// Override Dispose to ensure temporary file is deleted when viewer is closed.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            // Delete temp PDF file on close
            DisplayPdf(""); //Delete temp file
            base.OnClosed(e);
        }
    }
}