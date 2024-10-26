using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;


namespace ChordSheetConverter
{
    public class DocxToPdfConverter
    {
        private readonly string _libreOfficePath = @"C:\Program Files\LibreOffice\program\soffice.exe"; // Adjust this if LibreOffice is installed elsewhere

        // Method to check if Microsoft Word is installed
        public bool IsWordInstalled()
        {
            try
            {
                // Check if Word Interop can be accessed
                Type wordType = Type.GetTypeFromProgID("Word.Application");
                return wordType != null;
            }
            catch
            {
                return false;
            }
        }

        // Method to check if LibreOffice is installed
        public bool IsLibreOfficeInstalled()
        {
            return File.Exists(_libreOfficePath);
        }

        // Main method to convert DOCX to PDF using Word or LibreOffice
        public void ConvertDocxToPdf(string docxFilePath, string pdfOutputPath)
        {
            if (IsWordInstalled())
            {
                ConvertDocxToPdfUsingWord(docxFilePath, pdfOutputPath);
            }
            else if (IsLibreOfficeInstalled())
            {
                ConvertDocxToPdfUsingLibreOffice(docxFilePath, pdfOutputPath);
            }
            else
            {
                throw new Exception("Neither Microsoft Word nor LibreOffice is installed.");
            }
        }

        // Method to convert DOCX to PDF using Microsoft Word
        private static void ConvertDocxToPdfUsingWord(string docxFilePath, string pdfOutputPath)
        {
            Type wordType = Type.GetTypeFromProgID("Word.Application") ?? throw new Exception("Microsoft Word is not installed on this machine.");
            object wordApp = Activator.CreateInstance(wordType);
            object? wordDoc = null;

            try
            {
                // Set the Word application to be invisible
                wordType.InvokeMember("Visible", BindingFlags.SetProperty, null, wordApp, new object[] { false });

                // Open the DOCX file
                object documents = wordType.InvokeMember("Documents", BindingFlags.GetProperty, null, wordApp, null);
                wordDoc = documents.GetType().InvokeMember("Open", BindingFlags.InvokeMethod, null, documents, new object[] { docxFilePath });

                // Save the document as PDF
                object[] saveAsArgs = { pdfOutputPath, 17 /* WdSaveFormat.wdFormatPDF */ };
                wordDoc.GetType().InvokeMember("SaveAs2", BindingFlags.InvokeMethod, null, wordDoc, saveAsArgs);

                // Close the Word document
                wordDoc.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, wordDoc, new object[] { false });
            }
            catch (Exception ex)
            {
                throw new Exception("Error converting DOCX to PDF using Microsoft Word: " + ex.Message);
            }
            finally
            {
                // Release COM objects to prevent memory leaks
                if (wordDoc != null)
                {
                    Marshal.ReleaseComObject(wordDoc);
                }

                if (wordApp != null)
                {
                    wordType.InvokeMember("Quit", BindingFlags.InvokeMethod, null, wordApp, null);
                    Marshal.ReleaseComObject(wordApp);
                    wordApp = null;
                }

                // Ensure all Word processes are terminated
                KillWinWordProcesses();
            }

            // Force garbage collection twice to release any remaining COM references
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // Helper to kill lingering Word processes
        private static void KillWinWordProcesses()
        {
            foreach (var process in Process.GetProcessesByName("WINWORD"))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit();
                }
                catch
                {
                    // Ignore errors for processes that cannot be killed
                }
            }
        }

        // Method to convert DOCX to PDF using LibreOffice
        private void ConvertDocxToPdfUsingLibreOffice(string docxFilePath, string pdfOutputPath)
        {
            string outputDirectory = Path.GetDirectoryName(pdfOutputPath);

            try
            {
                // Step 1: Convert DOCX to PDF using LibreOffice
                ProcessStartInfo libreOfficeProcess = new()
                {
                    FileName = _libreOfficePath,
                    Arguments = $"--headless --convert-to pdf \"{docxFilePath}\" --outdir \"{outputDirectory}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                using (Process process = Process.Start(libreOfficeProcess))
                {
                    process.WaitForExit();
                }

                // Verify if the PDF was created
                if (!File.Exists(pdfOutputPath))
                {
                    throw new Exception("PDF file not generated by LibreOffice.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error converting DOCX to PDF using LibreOffice: " + ex.Message);
            }
        }
    }

}
