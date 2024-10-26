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
                    ReplacePropertiesInDocument(classInstance, wordDoc, body);

                    // Step 4: Replace placeholders in headers
                    foreach (var headerPart in documentPart.HeaderParts)
                    {
                        //                        ReplacePlaceholdersInPart(headerPart, xmlContent); // Ensure the headers are processed too
                        //ReplacePropertiesInDocument(classInstance, headerPart.RootElement); // Process class properties in headers
                    }

                    // Step 5: Replace placeholders in footers
                    foreach (var footerPart in documentPart.FooterParts)
                    {
                        //ReplacePlaceholdersInPart(footerPart, xmlContent); // Ensure the footers are processed too
                        //ReplacePropertiesInDocument(classInstance, footerPart.RootElement); // Process class properties in footers
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
                        paragraphs.Add((Paragraph)paragraph);
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
                        parentElement.AppendChild(new Run(new Text(combinedText)));
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

            // Find the original style in styles.xml
            var style = stylesPart.Styles.Elements<Style>().FirstOrDefault(s => s.StyleId == styleId);

            if (style != null && style.Type == StyleValues.Paragraph)
            {
                // Generate a unique character style ID and name
                string charStyleId = styleId + "Char";
                string charStyleName = "Character style for " + styleId;

                // Check if the character style already exists to avoid duplication
                var charStyle = stylesPart.Styles.Elements<Style>().FirstOrDefault(s => s.StyleId == charStyleId);
                if (charStyle == null)
                {
                    // Create a new character style
                    charStyle = new Style()
                    {
                        Type = StyleValues.Character,
                        StyleId = charStyleId,
                        CustomStyle = true
                    };

                    // Add style name and custom attributes for visibility in the styles pane
                    charStyle.Append(new StyleName() { Val = charStyleName });
                    charStyle.SetAttribute(new OpenXmlAttribute("w", "stylePaneSortKey", "http://schemas.openxmlformats.org/wordprocessingml/2006/main", "99"));
                    charStyle.SetAttribute(new OpenXmlAttribute("w", "stylePaneSortValue", "http://schemas.openxmlformats.org/wordprocessingml/2006/main", "Normal"));

                    // Get font-related run properties from the style hierarchy
                    StyleRunProperties? fontProperties = GetFontPropertiesFromStyleHierarchy(stylesPart, style) ?? GetDocumentDefaultFontProperties(stylesPart);

                    // Append the font properties to the character style
                    if (fontProperties != null)
                    {
                        charStyle.Append(fontProperties);
                    }
                    else
                    {
                        // Apply default font settings if none were found in the hierarchy
                        fontProperties = new StyleRunProperties();
                        fontProperties.Append(new RunFonts() { Ascii = "Arial", HighAnsi = "Arial" });
                        fontProperties.Append(new FontSize() { Val = "24" }); // 24 half-points = 12 pt font size
                        charStyle.Append(fontProperties);
                    }

                    // Append the new character style to the styles part
                    stylesPart.Styles.Append(charStyle);
                    stylesPart.Styles.Save();
                }

                // Apply the character style to the run
                RunProperties runPropertiesToApply = run.GetFirstChild<RunProperties>() ?? new RunProperties();
                runPropertiesToApply.Append(new RunStyle() { Val = charStyleId });

                if (!run.Elements<RunProperties>().Any())
                {
                    run.PrependChild(runPropertiesToApply);
                }
            }
            else
            {
                // Directly apply existing character style if available
                RunProperties runProperties = run.GetFirstChild<RunProperties>() ?? new RunProperties();
                runProperties.Append(new RunStyle() { Val = styleId });

                if (!run.Elements<RunProperties>().Any())
                {
                    run.PrependChild(runProperties);
                }
            }
        }



        private static StyleRunProperties GetFontPropertiesFromStyleHierarchy(StyleDefinitionsPart stylesPart, Style style)
        {
            while (style != null)
            {
                var runProperties = style.GetFirstChild<StyleRunProperties>();
                if (runProperties != null && runProperties.HasChildren)
                {
                    // Clone run properties and add color and frame if present
                    var fontProperties = (StyleRunProperties)runProperties.CloneNode(true);

                    // Include color if defined
                    var color = runProperties.GetFirstChild<Color>();
                    if (color != null)
                    {
                        fontProperties.Append(color.CloneNode(true));
                    }

                    // Include border properties if defined
                    AppendBorderProperties(runProperties, fontProperties);

                    return fontProperties;
                }

                // Check if the style is based on another style and follow the chain
                var basedOnStyleId = style.BasedOn?.Val;
                if (basedOnStyleId == null) break;

                style = stylesPart.Styles.Elements<Style>().FirstOrDefault(s => s.StyleId == basedOnStyleId);
            }
            return null;
        }


        // Helper to retrieve default font properties from document defaults
        private static StyleRunProperties GetDocumentDefaultFontProperties(StyleDefinitionsPart stylesPart)
        {
            var docDefaults = stylesPart.Styles.ChildElements
                .FirstOrDefault(e => e.LocalName == "docDefaults");

            if (docDefaults != null)
            {
                var runPropertiesDefault = docDefaults.Descendants<RunPropertiesDefault>().FirstOrDefault();
                if (runPropertiesDefault != null)
                {
                    var fontProperties = (StyleRunProperties)runPropertiesDefault.RunPropertiesBaseStyle.CloneNode(true);

                    // Include color if defined in document defaults
                    var color = runPropertiesDefault.RunPropertiesBaseStyle.GetFirstChild<Color>();
                    if (color != null)
                    {
                        fontProperties.Append(color.CloneNode(true));
                    }

                    // Include border properties from document defaults
                    AppendBorderProperties(runPropertiesDefault.RunPropertiesBaseStyle, fontProperties);

                    return fontProperties;
                }
            }
            return null;
        }


        private static void AppendBorderProperties(OpenXmlElement sourceProperties, StyleRunProperties targetProperties)
        {
            // Check for each border type and clone if present
            var topBorder = sourceProperties.GetFirstChild<TopBorder>();
            if (topBorder != null)
            {
                targetProperties.Append(topBorder.CloneNode(true));
            }

            var bottomBorder = sourceProperties.GetFirstChild<BottomBorder>();
            if (bottomBorder != null)
            {
                targetProperties.Append(bottomBorder.CloneNode(true));
            }

            var leftBorder = sourceProperties.GetFirstChild<LeftBorder>();
            if (leftBorder != null)
            {
                targetProperties.Append(leftBorder.CloneNode(true));
            }

            var rightBorder = sourceProperties.GetFirstChild<RightBorder>();
            if (rightBorder != null)
            {
                targetProperties.Append(rightBorder.CloneNode(true));
            }
        }





    }
}
