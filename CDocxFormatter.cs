using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Office.Interop.Word;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using static ChordSheetConverter.CAllConverters;
using static ChordSheetConverter.CChordSheetLine;
using FontFamily = System.Windows.Media.FontFamily;


namespace ChordSheetConverter
{
    /*
    public static class OpenXmlExtensions
    {
        // Helper method to replace text inside a paragraph, handling split text elements
        // Helper method to replace text inside a paragraph, handling split text elements
        public static void ReplaceText(this Paragraph paragraph, string oldText, string newText)
        {
            // Get all text elements in the paragraph
            var textElements = paragraph.Descendants<Text>().ToList();

            // Combine the text of all text elements
            string combinedText = string.Join("", textElements.Select(t => t.Text));

            // Check if the placeholder exists in the combined text
            string placeholder = "{" + oldText + "}"; // Include the curly braces in the placeholder
            if (combinedText.Contains(placeholder))
            {
                // If the newText is empty, remove the placeholder entirely
                if (string.IsNullOrEmpty(newText))
                {
                    combinedText = combinedText.Replace(placeholder, ""); // Remove the placeholder
                }
                else
                {
                    // Replace the placeholder with the new text
                    combinedText = combinedText.Replace(placeholder, newText); // Replace {title} with "Paradise"
                }

                // Now split the updated combined text back into individual text elements
                int charIndex = 0;
                foreach (var textElement in textElements)
                {
                    // Determine how much of the combinedText can fit into this text element
                    int lengthToTake = Math.Min(textElement.Text.Length, combinedText.Length - charIndex);
                    if (lengthToTake > 0)
                    {
                        // Replace the text element content with the corresponding part of the combined text
                        textElement.Text = combinedText.Substring(charIndex, lengthToTake);
                        charIndex += lengthToTake;
                    }
                    else
                    {
                        // Clear any remaining text elements
                        textElement.Text = string.Empty;
                    }
                }
            }
        }

    }
    */
    public class CDocxFormatter : CBasicConverter, IChordSheetAnalyzer
    {
        public Dictionary<string, string> propertyMapTags { get; } = [];

        private static readonly Dictionary<string, CFont> formattingRules = new Dictionary<string, CFont> {
            { "TextLine", new CFont (new FontFamily("Consolas"), 12.0, FontWeights.Normal, FontStyles.Normal, Colors.Black)},
            { "ChordLine", new CFont (new FontFamily("Consolas"), 12.0, FontWeights.Normal, FontStyles.Normal, Colors.Blue)},
            { "CommentLine", new CFont (new FontFamily("Consolas"), 12.0, FontWeights.Normal, FontStyles.Italic, Colors.Black)},
            { "SectionBegin", new CFont (new FontFamily("Consolas"), 12.0, FontWeights.Bold, FontStyles.Normal, Colors.Red)}        };


        public static readonly Dictionary<enLineType, string> LineTypeToStyleMap = new()
        {
        { enLineType.Unknown, "Standard" },        // Default or fallback style
        { enLineType.ChordLine, "SongChords" },    // Style for Chord lines
        { enLineType.TextLine, "SongText" },      // Style for Text lines
        { enLineType.EmptyLine, "Standard" },    // Style for Empty lines
        { enLineType.CommentLine, "SongComment" },// Style for Comment lines
        { enLineType.xmlElement, "Standard" },  // Style for XML elements
        { enLineType.xmlTagOpenTag, "Standard" },// Style for opening XML tags
        { enLineType.xmlTagClosingTag, "Standard" },// Style for closing XML tags
        { enLineType.SectionBegin, "SongSection" },// Style for section begins
        { enLineType.ColumnBreak, "Standard" },// Style for column breaks
        { enLineType.PageBreak, "Standard" }     // Style for page breaks
    };

        public static string replaceInTemplate<T>(string templatePath, string outputPath, T classInstance, List<CChordSheetLine> fullLyrics)
        {
            string ret = "";
            // Ensure the template file exists
            if (!File.Exists(templatePath))
            {
                return templatePath + " not found";
            }
            if (isFileInUse(templatePath))
            {
                return templatePath + " is open!!";
            }

            if (File.Exists(outputPath))
            {
                if (isFileInUse(outputPath))
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

                    // Step 1: Handle {SongBody} placeholder for lyrics insertion
                    var bodyTagParagraph = body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>()
                                               .Where(p => p.InnerText.Contains("{SongBody}"))
                                               .FirstOrDefault();

                    if (bodyTagParagraph != null)
                    {
                        List<DocumentFormat.OpenXml.Wordprocessing.Paragraph> newParagraphs = buildTextBlock(fullLyrics);

                        // Remove the placeholder paragraph
                        bodyTagParagraph.Remove();

                        // Insert the new paragraphs at the position where {Body} tag was
                        foreach (var newParagraph in newParagraphs)
                        {
                            body.AppendChild(newParagraph);  // Appends the new paragraph to the body
                        }
                    }

                    // Step 2: Replace all class properties in the document with matching placeholders
                    replacePropertiesInDocument(classInstance, body);

                    // Step 3: Replace placeholders in the main document with the XML content
                    //ReplacePlaceholdersInPart(documentPart, xmlContent);

                    // Step 4: Replace placeholders in headers
                    foreach (var headerPart in documentPart.HeaderParts)
                    {
//                        ReplacePlaceholdersInPart(headerPart, xmlContent); // Ensure the headers are processed too
                        replacePropertiesInDocument(classInstance, headerPart.RootElement); // Process class properties in headers
                    }

                    // Step 5: Replace placeholders in footers
                    foreach (var footerPart in documentPart.FooterParts)
                    {
                        //ReplacePlaceholdersInPart(footerPart, xmlContent); // Ensure the footers are processed too
                        replacePropertiesInDocument(classInstance, footerPart.RootElement); // Process class properties in footers
                    }

                    // Save the changes to the document
                    documentPart.Document.Save();
                }
            }
            return ret;
        }


        public static void replacePropertiesInDocument<T>(T classInstance, OpenXmlCompositeElement element)
        {
            // Get the properties of the class using reflection
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (var paragraph in element.Descendants<DocumentFormat.OpenXml.Wordprocessing.Paragraph>())
            {
                // Get all the text elements in the paragraph
                var textElements = paragraph.Descendants<Text>().ToList();

                // Combine the text of all text elements
                string combinedText = string.Join("", textElements.Select(t => t.Text));

                bool textUpdated = false;

                // Iterate over each property and replace its placeholder in the combined text
                foreach (var property in properties)
                {
                    // Construct the full placeholder with curly braces
                    string placeholder = "{" + property.Name + "}";
                    string propertyValue = property.GetValue(classInstance)?.ToString() ?? "";

                    // Replace or remove the placeholder
                    if (!string.IsNullOrEmpty(propertyValue))
                    {
                        combinedText = combinedText.Replace(placeholder, propertyValue);
                        textUpdated = true;
                    }
                    else
                    {
                        combinedText = combinedText.Replace(placeholder, ""); // Remove the placeholder
                        textUpdated = true;
                    }
                }

                // If the text was updated, split the combined text back into text elements
                if (textUpdated)
                {
                    int charIndex = 0;
                    foreach (var textElement in textElements)
                    {
                        int lengthToTake = Math.Min(textElement.Text.Length, combinedText.Length - charIndex);
                        if (lengthToTake > 0)
                        {
                            textElement.Text = combinedText.Substring(charIndex, lengthToTake);
                            charIndex += lengthToTake;
                        }
                        else
                        {
                            // Clear remaining text elements if the new combined text is shorter
                            textElement.Text = string.Empty;
                        }
                    }
                }
            }
        }


        // Method to build the text block from the list of tags and texts
        public static List<DocumentFormat.OpenXml.Wordprocessing.Paragraph> buildTextBlock(List<CChordSheetLine> textBlock)
        {
            var paragraphs = new List<DocumentFormat.OpenXml.Wordprocessing.Paragraph>();
            foreach (CChordSheetLine csl in textBlock)
            {
                enLineType tag = csl.lineType;
                string text = csl.line;

                string styleID = "Standard";
                if (LineTypeToStyleMap.ContainsKey(tag))
                    styleID = LineTypeToStyleMap[tag];

                

                if (tag == enLineType.EmptyLine)
                {
                    // Add an empty line
                    paragraphs.Add(new DocumentFormat.OpenXml.Wordprocessing.Paragraph(new Run()));
                }
                else if (tag == enLineType.PageBreak)
                {
                    // Add a page break
                    paragraphs.Add(createPageBreak());
                }
                else if (tag == enLineType.ColumnBreak)
                {
                    // Add a column break
                    paragraphs.Add(createColumnBreak());
                }
                else
                {
                    // Create a new paragraph with formatted text
                    //var paragraph = CreateFormattedParagraph(tag, text);
                    DocumentFormat.OpenXml.Wordprocessing.Paragraph? paragraph = null;

                    if (tag == CChordSheetLine.enLineType.ChordLine)
                    {
                        paragraph = createStyledChordLine(text, styleID);
                    }
                    if (tag == enLineType.CommentLine || tag == enLineType.TextLine)
                    {
                        paragraph = createStyledParagraph(text, styleID);
                    }
                    if (paragraph is not null)
                        paragraphs.Add((DocumentFormat.OpenXml.Wordprocessing.Paragraph) paragraph);
                }
            }

            return paragraphs;
        }

        // Helper method to create a page break
        private static DocumentFormat.OpenXml.Wordprocessing.Paragraph createPageBreak()
        {
            var run = new Run(new DocumentFormat.OpenXml.Wordprocessing.Break { Type = BreakValues.Page });
            return new DocumentFormat.OpenXml.Wordprocessing.Paragraph(run);
        }

        // Helper method to create a column break
        private static DocumentFormat.OpenXml.Wordprocessing.Paragraph createColumnBreak()
        {
            var run = new Run(new DocumentFormat.OpenXml.Wordprocessing.Break { Type = BreakValues.Column });
            return new DocumentFormat.OpenXml.Wordprocessing.Paragraph(run);
        }

        // Helper method to create a normal run with formatting
        private static Run createNormalRun(string text)
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
                string hexColor = CBasicConverter.convertColorToHex(font.FontColor);
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
        private static Run createSuperscriptRun(string number)
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
                string hexColor = CBasicConverter.convertColorToHex(font.FontColor);
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
        private static DocumentFormat.OpenXml.Wordprocessing.Paragraph createStyledParagraph(string text, string styleId)
        {
            var run = new Run(new Text(text)
            {
                Space = SpaceProcessingModeValues.Preserve // Preserve spaces in the text
            });

            var paragraphProperties = new ParagraphProperties();
            var paragraphStyleId = new ParagraphStyleId { Val = styleId }; // Set the style by its ID (name)
            paragraphProperties.Append(paragraphStyleId);

            var paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            paragraph.Append(paragraphProperties); // Apply the paragraph style
            paragraph.Append(run); // Add the text

            return paragraph;
        }


        // Helper method to create a chord line paragraph with a predefined style and superscript numbers
        private static DocumentFormat.OpenXml.Wordprocessing.Paragraph createStyledChordLine(string text, string styleId)
        {
            // Create a new Paragraph
            var paragraph = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();

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
                    paragraph.Append(createStyledRun(beforeNumber));
                }

                // Add the number as a superscript run
                paragraph.Append(createSuperscriptStyledRun(match.Value));

                // Update the last index processed
                lastIndex = match.Index + match.Length;
            }

            // Add any remaining text after the last number as a normal run
            if (lastIndex < text.Length)
            {
                string afterLastNumber = text.Substring(lastIndex);
                paragraph.Append(createStyledRun(afterLastNumber));
            }

            // Return the styled and formatted paragraph
            return paragraph;
        }

        // Helper method to create a normal run with the chord style
        private static Run createStyledRun(string text)
        {
            var run = new Run();

            // Preserve spaces in the text
            run.Append(new Text(text) { Space = SpaceProcessingModeValues.Preserve });

            return run;
        }

        // Helper method to create a superscript run for numbers
        private static Run createSuperscriptStyledRun(string number)
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

        public override string build(List<CChordSheetLine> chordSheetLines)
        {
            return getLines(chordSheetLines);
        }
    }
}



