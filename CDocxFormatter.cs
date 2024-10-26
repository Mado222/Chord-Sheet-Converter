using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using static ChordSheetConverter.CChordSheetLine;

namespace ChordSheetConverter
{
    public partial class CDocxFormatter : CBasicConverter
    {

        public static readonly Dictionary<EnLineType, string> LineTypeToStyleMap = new()
        {
        { EnLineType.Unknown, "Standard" },        // Default or fallback style
        { EnLineType.ChordLine, "songChords" },    // Style for Chord lines
        { EnLineType.TextLine, "songText" },      // Style for Text lines
        { EnLineType.EmptyLine, "Standard" },    // Style for Empty lines
        { EnLineType.CommentLine, "songComment" },// Style for Comment lines
        { EnLineType.xmlElement, "Standard" },  // Style for XML elements
        { EnLineType.xmlTagOpenTag, "Standard" },// Style for opening XML tags
        { EnLineType.xmlTagClosingTag, "Standard" },// Style for closing XML tags
        { EnLineType.SectionBegin, "songSection" },// Style for section begins
        { EnLineType.ColumnBreak, "Standard" },// Style for column breaks
        { EnLineType.PageBreak, "Standard" }     // Style for page breaks
    };


        public override Dictionary<string, string> PropertyMapTags => [];

        public static string ReplaceInTemplate<T>(string templatePath, string outputPath, T classInstance, List<CChordSheetLine> fullLyrics)
        {
            string ret = "";
            // Ensure the template file exists
            if (!File.Exists(templatePath))
            {
                return templatePath + " not found";
            }
            if (IsFileInUse(templatePath))
            {
                return templatePath + " is open!!";
            }

            if (File.Exists(outputPath))
            {
                if (IsFileInUse(outputPath))
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
                    if (body is null) return "";

                    // Step 1: Handle {SongBody} placeholder for lyrics insertion
                    var bodyTagParagraph = body.Descendants<Paragraph>().FirstOrDefault(p => p.InnerText.Contains("{songBody}"));

                    if (bodyTagParagraph != null)
                    {
                        List<Paragraph> newParagraphs = BuildTextBlock(fullLyrics);

                        // Remove the placeholder paragraph
                        bodyTagParagraph.Remove();

                        // Insert the new paragraphs at the position where {Body} tag was
                        foreach (var newParagraph in newParagraphs)
                        {
                            _ = body.AppendChild(newParagraph);  // Appends the new paragraph to the body
                        }
                    }

                    // Step 2: Replace all class properties in the document with matching placeholders
                    ReplacePropertiesInDocument(classInstance, wordDoc, body);

                    // Step 4: Replace placeholders in headers
                    foreach (var headerPart in documentPart.HeaderParts)
                    {
                        //ReplacePlaceholdersInPart(headerPart, xmlContent); // Ensure the headers are processed too
                        ReplacePropertiesInDocument(classInstance, wordDoc, headerPart.RootElement); // Process class properties in headers
                    }

                    // Step 5: Replace placeholders in footers
                    foreach (var footerPart in documentPart.FooterParts)
                    {
                        //ReplacePlaceholdersInPart(footerPart, xmlContent); // Ensure the footers are processed too
                        ReplacePropertiesInDocument(classInstance, wordDoc, footerPart.RootElement); // Process class properties in footers
                    }

                    // Save the changes to the document
                    documentPart.Document.Save();
                }
            }
            return ret;
        }

        public static List<Paragraph> BuildTextBlock(List<CChordSheetLine> textBlock)
        {
            var paragraphs = new List<Paragraph>();
            foreach (CChordSheetLine csl in textBlock)
            {
                EnLineType tag = csl.LineType;
                string text = csl.Line;

                // Determine the style ID based on the line type
                string styleID = "Standard";
                if (LineTypeToStyleMap.TryGetValue(tag, out string? value))
                    styleID = value;

                if (tag == EnLineType.EmptyLine)
                {
                    // Add an empty line
                    paragraphs.Add(new Paragraph(new Run()));
                }
                else if (tag == EnLineType.PageBreak)
                {
                    // Add a page break
                    paragraphs.Add(CreatePageBreak());
                }
                else if (tag == EnLineType.ColumnBreak)
                {
                    // Add a column break
                    paragraphs.Add(CreateColumnBreak());
                }
                else if (tag == EnLineType.ChordLine)
                {
                    // Create a styled paragraph specifically for chord lines
                    var paragraph = CreateStyledChordLine(text, styleID);
                    if (paragraph is not null)
                        paragraphs.Add(paragraph);
                }
                else if (tag == EnLineType.CommentLine || tag == EnLineType.TextLine)
                {
                    // Create a styled paragraph for comment or text lines
                    var paragraph = CreateStyledParagraph(text, styleID);
                    if (paragraph is not null)
                        paragraphs.Add(paragraph);
                }
            }

            return paragraphs;
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
            Regex regex = RegexCreateStyledParagraph();

            // Track the last index processed
            int lastIndex = 0;

            // Find and process all numbers in the text (for superscripting)
            var matches = regex.Matches(text);
            foreach (Match match in matches)
            {
                // Add the text before the number (if any) as a normal run
                if (match.Index > lastIndex)
                {
                    string beforeNumber = text[lastIndex..match.Index];
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
                string afterLastNumber = text[lastIndex..];
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

        [GeneratedRegex(@"\d+")]
        private static partial Regex RegexCreateStyledParagraph();
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

        public override string Build(List<CChordSheetLine> chordSheetLines)
        {
#if DEBUG
            FillPropertiesWithDefaults();
#endif
            return GetLines(chordSheetLines);
        }

        /*********************************************************/
        /******************* Replacements ************************/
        /*********************************************************/

        public static void ReplacePropertiesInDocument<T>(T classInstance, WordprocessingDocument wordDoc, OpenXmlCompositeElement element)
        {
            // Get the properties of the class using reflection
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (var paragraph in element.Descendants<Paragraph>())
            {
                if (paragraph == null) continue;

                // Get all the text elements in the paragraph
                var textElements = paragraph.Descendants<Text>().ToList();
                if (textElements.Count > 0)
                {
                    // Find the parent Run or Paragraph element for the text
                    OpenXmlElement parentElement = FindSuitableParentFromParagraph(paragraph);
                    if (parentElement == null) continue;

                    // Concatenate the text content from the existing textElements list
                    string combinedText = string.Concat(textElements.Select(te => te.Text));

                    // Track if the text was updated
                    bool textUpdated = false;

                    // Iterate through the properties and process replacements
                    foreach (var property in properties)
                    {
                        string placeholder = "{song" + property.Name + "}";
                        string propertyValue = property.GetValue(classInstance)?.ToString() ?? "";

                        // If the placeholder is found, replace it
                        if (combinedText.Contains(placeholder))
                        {
                            combinedText = combinedText.Replace(placeholder, propertyValue);
                            textUpdated = true;

                            // Create a new Run element with the text content
                            Run newRun = new(new Text(propertyValue));

                            // Apply the user's defined style, checking and converting if needed
                            ApplyUserDefinedStyle(wordDoc, newRun, "song" + property.Name);

                            parentElement.AppendChild(newRun);
                        }
                    }

                    // If text was updated, remove the original text elements
                    if (textUpdated)
                    {
                        foreach (var textElement in textElements)
                        {
                            textElement.Remove();
                        }
                    }
                    else
                    {
                        // If no replacement was done, add the unmodified combined text
                        // parentElement.AppendChild(new Run(new Text(combinedText)));
                    }
                }
            }
        }

        private static OpenXmlElement FindSuitableParentFromParagraph(Paragraph paragraph)
        {
            // Look for a Run or suitable parent in the paragraph
            OpenXmlElement parentElement = paragraph.Descendants<Run>().FirstOrDefault();
            return parentElement ?? (OpenXmlElement)paragraph;
        }

        // Helper method to apply a user-defined style, converting paragraph styles if needed
        private static void ApplyUserDefinedStyle(WordprocessingDocument wordDoc, Run run, string styleId)
        {
            var stylesPart = wordDoc.MainDocumentPart.StyleDefinitionsPart;
            if (stylesPart == null) return;

            // Append "Zchn" to use the character style variant
            styleId += "Zchn";

            // Find the character style in styles.xml
            Style? style = stylesPart.Styles.Elements<Style>().FirstOrDefault(s => s.StyleId == styleId);

            // Fallback to "Standard" if style not found
            if (style == null)
            {
                styleId = "Standard";
            }

            // Apply the character style directly to the run
            RunProperties runProperties = run.GetFirstChild<RunProperties>() ?? new RunProperties();
            runProperties.Append(new RunStyle() { Val = styleId });

            if (!run.Elements<RunProperties>().Any())
            {
                run.PrependChild(runProperties);
            }
        }
    }
}
