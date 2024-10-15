using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;


namespace ChordSheetConverter
{
    public class MyRichTextBox : RichTextBox
    {
        public string text
        {
            get
            {
                return getPlainText();
            }
            set
            {
                setPlainText(value);
            }
        }

        public string rtfText
        {
            get
            {
                return getRtfText();
            }
            set
            {
                setRtfText(value);
            }
        }


        // Get plain text from RichTextBox
        public string getPlainText()
        {
            TextRange textRange = new(this.Document.ContentStart, this.Document.ContentEnd);
            return textRange.Text;
        }

        // Set plain text in RichTextBox
        public void setPlainText(string text)
        {
            this.Document.Blocks.Clear();
            this.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        // Get RTF formatted text from RichTextBox
        public string getRtfText()
        {
            TextRange textRange = new(this.Document.ContentStart, this.Document.ContentEnd);

            using (MemoryStream stream = new())
            {
                textRange.Save(stream, DataFormats.Rtf);
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        // Set RTF formatted text in RichTextBox
        public void setRtfText(string rtfText)
        {
            using (MemoryStream stream = new(System.Text.Encoding.UTF8.GetBytes(rtfText)))
            {
                TextRange textRange = new(this.Document.ContentStart, this.Document.ContentEnd);
                textRange.Load(stream, DataFormats.Rtf);
            }
        }

        // Save plain text to a file
        public void savePlainTextToFile(string filePath)
        {
            string plainText = getPlainText();
            File.WriteAllText(filePath, plainText);
        }

        // Load plain text from a file
        public void loadPlainTextFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string text = File.ReadAllText(filePath);
                setPlainText(text);
            }
            else
            {
                MessageBox.Show("File does not exist.");
            }
        }

        // Save RTF formatted text to a file
        public void saveRtfTextToFile(string filePath)
        {
            string rtfText = getRtfText();
            File.WriteAllText(filePath, rtfText);
        }

        // Load RTF formatted text from a file
        public void loadRtfTextFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                string rtfText = File.ReadAllText(filePath);
                setRtfText(rtfText);
            }
            else
            {
                MessageBox.Show("File does not exist.");
            }
        }
    }
}
