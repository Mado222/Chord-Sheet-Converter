using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using ColorOpenXml = DocumentFormat.OpenXml.Wordprocessing.Color; // Alias for OpenXml Color
using FontFamily = System.Windows.Media.FontFamily;



namespace ChordSheetConverter
{
    public class CDocxFormatter
    {
        private static Dictionary<string, CFont> formattingRules = new Dictionary<string, CFont> {
            { "TextLine", new CFont (new FontFamily("Consolas"), 12.0, FontWeights.Normal, FontStyles.Normal, Colors.Black)},
            { "ChordLine", new CFont (new FontFamily("Consolas"), 12.0, FontWeights.Normal, FontStyles.Normal, Colors.Blue)},
            { "CommentLine", new CFont (new FontFamily("Consolas"), 12.0, FontWeights.Normal, FontStyles.Italic, Colors.Black)},
            { "SectionBegin", new CFont (new FontFamily("Consolas"), 12.0, FontWeights.Bold, FontStyles.Normal, Colors.Red)}        };

        private static Dictionary<string, string> chordProTagsToWordStyles = new()
        {
            {"Title", "SongTitle"},
            { "Author", "SongAuthor"},
            { "TextLine", "SongText"},
            { "ChordLine","SongChords"},
            { "CommentLine", "SongComment"},
            { "SectionBegin", "SongSection"},
            { "EmptyLine", "Standard"}
        };


        public static string ReplaceInTemplate(string templatePath, string outputPath, Dictionary<string, string> xmlContent, List<(string tag, string text)> fullLyrics)
        {
            string ret = "";
            // Ensure the template file exists
            if (!File.Exists(templatePath))
            {
                return templatePath + " not found";
            }
            if (CBasicConverter.isFileInUse(templatePath))
            {
                return templatePath + " is open!!";
            }

            if (File.Exists(outputPath))
            {
                if (CBasicConverter.isFileInUse(outputPath))
                {
                    return outputPath + " is open!!";
                }
            }

            // Copy the template to the output path
            File.Copy(templatePath, outputPath, true); // true to overwrite if the file exists

            // Open the copied document for editing
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(outputPath, true))
            {
                // Get the document's main body content
                var documentPart = wordDoc.MainDocumentPart;

                if (documentPart != null)
                {
                    var body = documentPart.Document.Body;
                    // Find the paragraph that contains the {SongBody} tag
                    var bodyTagParagraph = body.Descendants<Paragraph>()
                                               .Where(p => p.InnerText.Contains("{SongBody}"))
                                               .FirstOrDefault();

                    if (bodyTagParagraph != null)
                    {
                        List<Paragraph> newParagraphs = BuildTextBlock(fullLyrics);

                        // Remove the placeholder paragraph
                        bodyTagParagraph.Remove();

                        // Insert the new paragraphs at the position where {Body} tag was
                        foreach (var newParagraph in newParagraphs)
                        {
                            body.AppendChild(newParagraph);  // Appends the new paragraph to the body
                        }
                    }
                    documentPart.Document.Save();

                    // Replace the placeholders in the main document
                    ReplacePlaceholdersInPart(documentPart, xmlContent);

                    // Replace placeholders in headers
                    foreach (var headerPart in documentPart.HeaderParts)
                    {
                        ReplacePlaceholdersInPart(headerPart, xmlContent);
                    }

                    // Replace placeholders in footers
                    foreach (var footerPart in documentPart.FooterParts)
                    {
                        ReplacePlaceholdersInPart(footerPart, xmlContent);
                    }

                    // Save the changes to the document
                    documentPart.Document.Save();
                }
            }
            return ret;
        }

        private static void ReplacePlaceholdersInPart(OpenXmlPart part, Dictionary<string, string> replacements)
        {
            // Get all the text elements in the part
            var textElements = part.RootElement.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>();

            // Iterate through the text elements and replace the placeholders
            foreach (var textElement in textElements)
            {
                foreach (var replacement in replacements)
                {
                    if (textElement.Text.Contains(replacement.Key))
                    {
                        textElement.Text = textElement.Text.Replace(replacement.Key, replacement.Value);
                    }
                }
            }
        }

        /*
        public static List<Paragraph> createFormattedTextBlock(List<Tuple<string, CFont>> textBlockContent)
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

        public static void replacePlaceholderWithFormattedBlock(MainDocumentPart documentPart, string placeholder, List<Paragraph> formattedParagraphs)
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

        public static void appendFormattedParagraph(MainDocumentPart documentPart, string textContent, CFont fontSettings)
        {
            // Create a new Paragraph
            Paragraph paragraph = new();
            Run run = new();
            Text text = new(textContent);

            // Add the text to the Run
            run.Append(text);

            // Create RunProperties to apply formatting
            RunProperties runProperties = new();

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

        private static void replaceTextInDocument(OpenXmlCompositeElement element, Dictionary<string, string> replacements)
        {
            // Iterate through the replacements dictionary
            foreach (var replacement in replacements)
            {
                // Get all Text elements in the document element (Body, Header, or Footer)
                var textElements = element.Descendants<Text>();

                // Replace placeholder tags in the text
                foreach (var textElement in textElements)
                {
                    if (textElement.Text.Contains(replacement.Key))
                    {
                        textElement.Text = textElement.Text.Replace(replacement.Key, replacement.Value);
                    }
                }
            }
        }
        */


        // Method to build the text block from the list of tags and texts
        public static List<Paragraph> BuildTextBlock(List<(string tag, string text)> textBlock)
        {
            var paragraphs = new List<Paragraph>();
            foreach (var (tag, text) in textBlock)
            {
                string styleID = "Standard";
                if (chordProTagsToWordStyles.ContainsKey(tag))
                    styleID = chordProTagsToWordStyles[tag];

                if (tag == "EmptyLine")
                {
                    // Add an empty line
                    paragraphs.Add(new Paragraph(new Run()));
                }
                else if (tag == "PageBreak")
                {
                    // Add a page break
                    paragraphs.Add(CreatePageBreak());
                }
                else if (tag == "ColumnBreak")
                {
                    // Add a column break
                    paragraphs.Add(CreateColumnBreak());
                }
                else
                {
                    // Create a new paragraph with formatted text
                    //var paragraph = CreateFormattedParagraph(tag, text);
                    Paragraph paragraph;
                    if (tag == "ChordLine")
                    {
                        paragraph = CreateStyledChordLine(text, styleID);
                    }
                    else
                    {
                        paragraph = CreateStyledParagraph(text, styleID);
                    }
                        paragraphs.Add(paragraph);
                }
            }

            return paragraphs;
        }

        // Helper method to create a formatted paragraph based on the tag
        private static Paragraph CreateFormattedParagraph(string tag, string text)
        {
            var run = new Run();
            var runProperties = new RunProperties();

            if (formattingRules.ContainsKey(tag))
            {
                var font = formattingRules[tag];

                // Set the font family
                runProperties.Append(new RunFonts { Ascii = font.FontFamily.Source });

                // Set font size (OpenXML uses half-points, so multiply by 2)
                runProperties.Append(new FontSize { Val = (font.FontSize * 2).ToString() });

                // Set font weight (bold)
                if (font.FontWeight == FontWeights.Bold)
                {
                    runProperties.Append(new Bold());
                }

                // Set font style (italic)
                if (font.FontStyle == FontStyles.Italic)
                {
                    runProperties.Append(new Italic());
                }

                // Set font color (convert WPF color to hex)
                string hexColor = CBasicConverter.ConvertColorToHex(font.FontColor);
                runProperties.Append(new DocumentFormat.OpenXml.Wordprocessing.Color { Val = hexColor });
            }

            // Add the text content to the run, ensuring leading spaces are preserved
            var textElement = new Text(text)
            {
                Space = SpaceProcessingModeValues.Preserve // Preserves leading spaces
            };

            // Add the text element to the run
            run.Append(runProperties);
            run.Append(textElement);

            // Create and return the paragraph
            return new Paragraph(run);
        }

        // Helper method to create a page break
        private static Paragraph CreatePageBreak()
        {
            var run = new Run(new Break { Type = BreakValues.Page });
            return new Paragraph(run);
        }

        // Helper method to create a column break
        private static Paragraph CreateColumnBreak()
        {
            var run = new Run(new Break { Type = BreakValues.Column });
            return new Paragraph(run);
        }

        private static Paragraph CreateFormattedChordLine(string text)
        {
            var run = new Run();
            var runProperties = new RunProperties();

            var paragraph = new Paragraph();

            // Regular expression to find numbers
            Regex regex = new Regex(@"\d+");

            // Split the chord line by numbers
            var matches = regex.Matches(text);
            int lastIndex = 0;

            foreach (Match match in matches)
            {
                // Add the text before the number (if any) as a normal run
                if (match.Index > lastIndex)
                {
                    string beforeNumber = text.Substring(lastIndex, match.Index - lastIndex);
                    paragraph.Append(CreateNormalRun(beforeNumber));
                }

                // Add the number as a superscript run
                paragraph.Append(CreateSuperscriptRun(match.Value));

                // Update the last processed index
                lastIndex = match.Index + match.Length;
            }

            // Add any remaining text after the last number as a normal run
            if (lastIndex < text.Length)
            {
                string afterLastNumber = text.Substring(lastIndex);
                paragraph.Append(CreateNormalRun(afterLastNumber));
            }

            return paragraph;
        }

        // Helper method to create a normal run with formatting
        private static Run CreateNormalRun(string text)
        {
            var run = new Run();
            var runProperties = new RunProperties();

            // Apply formatting if available (use "ChordLine" as an example for now)
            if (formattingRules.ContainsKey("ChordLine"))  // Adjust the tag based on your needs
            {
                var font = formattingRules["ChordLine"];

                // Set the font family
                runProperties.Append(new RunFonts { Ascii = font.FontFamily.Source });

                // Set font size (OpenXML uses half-points, so multiply by 2)
                runProperties.Append(new FontSize { Val = (font.FontSize * 2).ToString() });

                // Set bold if the font weight is bold
                if (font.FontWeight == FontWeights.Bold)
                {
                    runProperties.Append(new Bold());
                }

                // Set italic if the font style is italic
                if (font.FontStyle == FontStyles.Italic)
                {
                    runProperties.Append(new Italic());
                }

                // Set the font color (convert WPF Color to hex)
                string hexColor = CBasicConverter.ConvertColorToHex(font.FontColor);
                runProperties.Append(new DocumentFormat.OpenXml.Wordprocessing.Color { Val = hexColor });
            }

            run.Append(runProperties);

            // Append the text with preserved spaces
            run.Append(new Text(text)
            {
                Space = SpaceProcessingModeValues.Preserve // Ensure spaces are preserved
            });

            return run;
        }

        // Helper method to create a superscript run with formatting
        private static Run CreateSuperscriptRun(string number)
        {
            var run = new Run();
            var runProperties = new RunProperties
            {
                VerticalTextAlignment = new VerticalTextAlignment { Val = VerticalPositionValues.Superscript }
            };

            // Apply formatting if available (use "ChordLine" as an example for now)
            if (formattingRules.ContainsKey("ChordLine"))  // Adjust the tag based on your needs
            {
                var font = formattingRules["ChordLine"];

                // Set the font family
                runProperties.Append(new RunFonts { Ascii = font.FontFamily.Source });

                // Set font size (OpenXML uses half-points, so multiply by 2)
                runProperties.Append(new FontSize { Val = (font.FontSize * 2).ToString() });

                // Set bold if the font weight is bold
                if (font.FontWeight == FontWeights.Bold)
                {
                    runProperties.Append(new Bold());
                }

                // Set italic if the font style is italic
                if (font.FontStyle == FontStyles.Italic)
                {
                    runProperties.Append(new Italic());
                }

                // Set the font color (convert WPF Color to hex)
                string hexColor = CBasicConverter.ConvertColorToHex(font.FontColor);
                runProperties.Append(new DocumentFormat.OpenXml.Wordprocessing.Color { Val = hexColor });
            }

            run.Append(runProperties);

            // Append the number with preserved spaces
            run.Append(new Text(number)
            {
                Space = SpaceProcessingModeValues.Preserve // Ensure spaces are preserved
            });

            return run;
        }

        // Helper method to create a paragraph with a style
        private static Paragraph CreateStyledParagraph(string text, string styleId)
        {
            var run = new Run(new Text(text)
            {
                Space = SpaceProcessingModeValues.Preserve // Preserve spaces in the text
            });

            var paragraphProperties = new ParagraphProperties();
            var paragraphStyleId = new ParagraphStyleId { Val = styleId }; // Set the style by its ID (name)
            paragraphProperties.Append(paragraphStyleId);

            var paragraph = new Paragraph();
            paragraph.Append(paragraphProperties); // Apply the paragraph style
            paragraph.Append(run); // Add the text

            return paragraph;
        }


        // Helper method to create a chord line paragraph with a predefined style and superscript numbers
        private static Paragraph CreateStyledChordLine(string text, string styleId)
        {
            // Create a new Paragraph
            var paragraph = new Paragraph();

            // Create paragraph properties (this is where the style is applied)
            var paragraphProperties = new ParagraphProperties();

            // Set the style ID to "ChordStyle" (or the style name defined in your Word template)
            var paragraphStyleId = new ParagraphStyleId { Val = styleId }; // Replace with the actual style name
            paragraphProperties.Append(paragraphStyleId);

            // Apply the paragraph properties (style) to the paragraph
            paragraph.Append(paragraphProperties);

            // Regular expression to find numbers in the text
            Regex regex = new Regex(@"\d+");

            // Track the last index processed
            int lastIndex = 0;

            // Find and process all numbers in the text (for superscripting)
            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                // Add the text before the number (if any) as a normal run
                if (match.Index > lastIndex)
                {
                    string beforeNumber = text.Substring(lastIndex, match.Index - lastIndex);
                    paragraph.Append(CreateStyledRun(beforeNumber));
                }

                // Add the number as a superscript run
                paragraph.Append(CreateSuperscriptStyledRun(match.Value));

                // Update the last index processed
                lastIndex = match.Index + match.Length;
            }

            // Add any remaining text after the last number as a normal run
            if (lastIndex < text.Length)
            {
                string afterLastNumber = text.Substring(lastIndex);
                paragraph.Append(CreateStyledRun(afterLastNumber));
            }

            // Return the styled and formatted paragraph
            return paragraph;
        }

        // Helper method to create a normal run with the chord style
        private static Run CreateStyledRun(string text)
        {
            var run = new Run();

            // Preserve spaces in the text
            run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

            return run;
        }

        // Helper method to create a superscript run for numbers
        private static Run CreateSuperscriptStyledRun(string number)
        {
            var run = new Run();

            // Set the text as superscript
            var runProperties = new RunProperties
            {
                VerticalTextAlignment = new VerticalTextAlignment { Val = VerticalPositionValues.Superscript }
            };

            run.Append(runProperties);

            // Add the number to the run
            run.Append(new Text(number) { Space = SpaceProcessingModeValues.Preserve });

            return run;
        }

    }
}



