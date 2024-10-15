using System.Windows;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Packaging;
using ColorOpenXml = DocumentFormat.OpenXml.Wordprocessing.Color; // Alias for OpenXml Color


namespace ChordSheetConverter
{
    public class CDocxFormatter
    {
        public List<Paragraph> CreateFormattedTextBlock(List<Tuple<string, CFont>> textBlockContent)
        {
            List<Paragraph> formattedParagraphs = [];

            foreach (var content in textBlockContent)
            {
                string text = content.Item1;
                CFont fontSettings = content.Item2;

                // Create a new Paragraph for each line of text
                Paragraph paragraph = new();
                Run run = new();
                Text txt = new(text);
                run.Append(txt);

                // Create RunProperties for formatting
                RunProperties runProperties = new();

                // Apply font settings
                if (fontSettings != null)
                {
                    // Font family
                    RunFonts runFonts = new() { Ascii = fontSettings.FontFamily.Source };
                    runProperties.Append(runFonts);

                    // Font size (OpenXML uses half-points, so multiply by 2)
                    runProperties.Append(new FontSize { Val = (fontSettings.FontSize * 2).ToString() });

                    // Font weight (Bold)
                    if (fontSettings.FontWeight == FontWeights.Bold)
                        runProperties.Append(new Bold());

                    // Font style (Italic)
                    if (fontSettings.FontStyle == FontStyles.Italic)
                        runProperties.Append(new Italic());

                    // Font color
                    string fontColorHex = fontSettings.FontColor.ToString().Substring(3, 6); // Remove the alpha value
                    runProperties.Append(new ColorOpenXml { Val = fontColorHex });
                }

                // Add RunProperties to the Run
                run.PrependChild(runProperties);

                // Add the Run to the Paragraph
                paragraph.Append(run);

                // Add formatted paragraph to the list
                formattedParagraphs.Add(paragraph);
            }

            return formattedParagraphs;
        }

        public void ReplacePlaceholderWithFormattedBlock(MainDocumentPart documentPart, string placeholder, List<Paragraph> formattedParagraphs)
        {
            // Find the paragraph containing the placeholder (e.g., {lyrics})
            var paragraphs = documentPart.Document.Body.Elements<Paragraph>();

            foreach (var paragraph in paragraphs)
            {
                if (paragraph.InnerText.Contains(placeholder))
                {
                    // Remove the placeholder paragraph
                    paragraph.Remove();

                    // Insert the new formatted paragraphs
                    foreach (var formattedParagraph in formattedParagraphs)
                    {
                        documentPart.Document.Body.Append(formattedParagraph);
                    }

                    // Save changes to the document
                    documentPart.Document.Save();
                    break;
                }
            }
        }


        public void AppendFormattedParagraph(MainDocumentPart documentPart, string textContent, CFont fontSettings)
        {
            // Create a new Paragraph
            Paragraph paragraph = new ();
            Run run = new ();
            Text text = new (textContent);

            // Add the text to the Run
            run.Append(text);

            // Create RunProperties to apply formatting
            RunProperties runProperties = new ();

            // Apply font settings
            if (fontSettings != null)
            {
                // Font family
                RunFonts runFonts = new() { Ascii = fontSettings.FontFamily.Source };
                runProperties.Append(runFonts);

                // Font size
                runProperties.Append(new FontSize { Val = (fontSettings.FontSize * 2).ToString() }); // FontSize requires value in half-points

                // Font weight (Bold)
                if (fontSettings.FontWeight == FontWeights.Bold)
                {
                    runProperties.Append(new Bold());
                }

                // Font style (Italic)
                if (fontSettings.FontStyle == FontStyles.Italic)
                {
                    runProperties.Append(new Italic());
                }

                // Font color
                string fontColorHex = fontSettings.FontColor.ToString().Substring(3, 6); // Remove the alpha value from System.Windows.Media.Color
                runProperties.Append(new ColorOpenXml { Val = fontColorHex }); // Use the alias for OpenXml color
            }

            // Add the RunProperties to the Run
            run.PrependChild(runProperties);

            // Add the Run to the Paragraph
            paragraph.Append(run);

            // Append the Paragraph to the document body
            documentPart.Document.Body.Append(paragraph);

            // Save changes to the document
            documentPart.Document.Save();
        }

        public void ReplaceTagsInDocx(string docxFilePath, Dictionary<string, string> replacements)
        {
            // Open the DOCX file for editing
            using (WordprocessingDocument doc = WordprocessingDocument.Open(docxFilePath, true))
            {
                // Access the main document part
                var documentPart = doc.MainDocumentPart;
                if (documentPart != null)
                {
                    // Get all the text in the document
                    var docText = documentPart.Document.Body.InnerText;

                    // Iterate through the replacements dictionary
                    foreach (var replacement in replacements)
                    {
                        // Find and replace the placeholder tags in the document text
                        ReplaceTextInDocument(documentPart, replacement.Key, replacement.Value);
                    }

                    // Save the changes
                    documentPart.Document.Save();
                }
            }
        }

        private void ReplaceTextInDocument(MainDocumentPart documentPart, string placeholder, string replacementText)
        {
            // Find all Text elements in the document
            var textElements = documentPart.Document.Descendants<Text>();

            // Replace the placeholder text with the desired replacement
            foreach (var textElement in textElements)
            {
                if (textElement.Text.Contains(placeholder))
                {
                    textElement.Text = textElement.Text.Replace(placeholder, replacementText);
                }
            }
        }
    }
}


