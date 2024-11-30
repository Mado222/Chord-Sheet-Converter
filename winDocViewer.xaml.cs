using System.ComponentModel;
using System.Windows;

namespace ChordSheetConverter
{
    public partial class DocViewerWindow : Window
    {
        public DocViewerWindow()
        {
            InitializeComponent();
        }

        private void DocViewerWindow_Closing(object sender, CancelEventArgs e)
        {
            // Cancel the close operation
            e.Cancel = true;

            // Hide the window instead of closing
            this.Hide();
        }

        public void DisplayPdf(string pdfPath)
        {
            // Load the PDF in the WebBrowser control
            webBrowserPdf.Navigate(pdfPath);
        }
    }
}
