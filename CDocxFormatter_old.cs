using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using static ChordSheetConverter.CChordSheetLine;


namespace ChordSheetConverter
{
    public partial class CDocxFormatterx : CBasicConverter
    {

        public static readonly Dictionary<enLineType, string> LineTypeToStyleMap = new()
        {
        { enLineType.Unknown, "Standard" },        // Default or fallback style
        { enLineType.ChordLine, "songChords" },    // Style for Chord lines
        { enLineType.TextLine, "songText" },      // Style for Text lines
        { enLineType.EmptyLine, "Standard" },    // Style for Empty lines
        { enLineType.CommentLine, "songComment" },// Style for Comment lines
        { enLineType.xmlElement, "Standard" },  // Style for XML elements
        { enLineType.xmlTagOpenTag, "Standard" },// Style for opening XML tags
        { enLineType.xmlTagClosingTag, "Standard" },// Style for closing XML tags
        { enLineType.SectionBegin, "songSection" },// Style for section begins
        { enLineType.ColumnBreak, "Standard" },// Style for column breaks
        { enLineType.PageBreak, "Standard" }     // Style for page breaks
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
                            body.AppendChild(newParagraph);  // Appends the new paragraph to the body
                        }
                    }

                    // Step 2: Replace all class properties in the document with matching placeholders
                    ReplacePropertiesInDocument(classInstance, body);

                    // Step 4: Replace placeholders in headers
                    foreach (var headerPart in documentPart.HeaderParts)
                    {
//                        ReplacePlaceholdersInPart(headerPart, xmlContent); // Ensure the headers are processed too
                        ReplacePropertiesInDocument(classInstance, headerPart.RootElement); // Process class properties in headers
                    }

                    // Step 5: Replace placeholders in footers
                    foreach (var footerPart in documentPart.FooterParts)
                    {
                        //ReplacePlaceholdersInPart(footerPart, xmlContent); // Ensure the footers are processed too
                        ReplacePropertiesInDocument(classInstance, footerPart.RootElement); // Process class properties in footers
                    }

                    // Save the changes to the document
                    documentPart.Document.Save();
                }
            }
            return ret;
        }

        public static void ReplacePropertiesInDocument<T>(T classInstance, OpenXmlCompositeElement element)
        {
            // Get the properties of the class using reflection
            PropertyInfo[] properties = typeof(T).GetProperties();

            foreach (var paragraph in element.Descendants<Paragraph>())
            {
                // Get all the text elements in the paragraph
                var textElements = paragraph.Descendants<Text>().ToList();
                if (textElements.Count > 0)
                {
                    // Find the parent Run or Paragraph element for the text
                    OpenXmlElement parentElement = FindSuitableParentFromParagraph(paragraph);
                    if (parentElement == null)
                    {
                        throw new InvalidOperationException("No suitable parent (Run or Paragraph) found for text elements.");
                    }

                    // Combine the text of all text elements into one string
                    string combinedText = string.Join("", textElements.Select(t => t.Text));

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

                            // Create a new Run element and insert it with the text and style
                            Run newRun = new Run(new Text(propertyValue));
                            ApplyStyleToRun(newRun, "song" + property.Name);
                            parentElement.AppendChild(newRun);
                        }
                    }

                    // If the text was updated, remove the original text elements
                    if (textUpdated)
                    {
                        foreach (var textElement in textElements)
                        {
                            textElement.Remove();
                        }

                        // Add any remaining text (after replacements)
                        if (!string.IsNullOrEmpty(combinedText))
                        {
                            parentElement.AppendChild(new Run(new Text(combinedText)));
                        }
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

        private static void ApplyStyleToRun(Run run, string styleName)
        {
            // Create or retrieve the RunProperties for the Run
            RunProperties runProperties = run.GetFirstChild<RunProperties>() ?? new RunProperties();

            // Remove any existing RunStyle to avoid conflicts
            var existingRunStyle = runProperties.GetFirstChild<RunStyle>();
            if (existingRunStyle != null)
            {
                existingRunStyle.Remove();
            }

            // Apply the specified style to the Run
            RunStyle runStyle = new RunStyle() { Val = styleName };
            runProperties.Append(runStyle);

            // Append RunProperties to the Run if not already added
            if (!run.Elements<RunProperties>().Any())
            {
                run.PrependChild(runProperties);
            }
        }
















        // Method to build the text block from the list of tags and texts
        public static List<Paragraph> BuildTextBlock(List<CChordSheetLine> textBlock)
        {
            var paragraphs = new List<Paragraph>();
            foreach (CChordSheetLine csl in textBlock)
            {
                enLineType tag = csl.lineType;
                string text = csl.line;

                string styleID = "Standard";
                if (LineTypeToStyleMap.TryGetValue(tag, out string? value))
                    styleID = value;

                if (tag == enLineType.EmptyLine)
                {
                    // Add an empty line
                    paragraphs.Add(new Paragraph(new Run()));
                }
                else if (tag == enLineType.PageBreak)
                {
                    // Add a page break
                    paragraphs.Add(CreatePageBreak());
                }
                else if (tag == enLineType.ColumnBreak)
                {
                    // Add a column break
                    paragraphs.Add(CreateColumnBreak());
                }
                else
                {
                    // Create a new paragraph with formatted text
                    //var paragraph = CreateFormattedParagraph(tag, text);
                    Paragraph? paragraph = null;

                    if (tag == enLineType.ChordLine)
                    {
                        paragraph = CreateStyledChordLine(text, styleID);
                    }
                    if (tag == enLineType.CommentLine || tag == enLineType.TextLine)
                    {
                        paragraph = CreateStyledParagraph(text, styleID);
                    }
                    if (paragraph is not null)
                        paragraphs.Add((Paragraph) paragraph);
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

        // Helper method to create a normal run with formatting
        /*private static Run CreateNormalRun(string text)
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
                runProperties.Append(new Color { Val = hexColor });
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
                runProperties.Append(new Color { Val = hexColor });
            }

            run.Append(runProperties);

            // Append the number with preserved spaces
            run.Append(new Text(number)
            {
                Space = SpaceProcessingModeValues.Preserve // Ensure spaces are preserved
            });

            return run;
        }*/

        [GeneratedRegex(@"\d+")]
        private static partial Regex RegexCreateStyledParagraphx();
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
            Regex regex = RegexCreateStyledParagraphx();

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

        public override string Build(List<CChordSheetLine> chordSheetLines)
        {
#if DEBUG
            FillPropertiesWithDefaults();
#endif
            return GetLines(chordSheetLines);
        }

    }
}



