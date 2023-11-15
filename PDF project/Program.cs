using System.Reflection.PortableExecutable;
using System;
using System.Net.Http;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.IO;
using System.Reflection.Metadata;

namespace PDF_project
{
    class Program
    {
        static void Main()
        {
            // Replace the URL with the actual PDF file URL
            // 177262
            string pdfUrl = "https://va.lvceli.lv/Request/request/Application/GetPermissionPdfFile?id=177659";

            using (HttpClient client = new HttpClient())
            {
                // Download the PDF file as a byte array
                byte[] pdfBytes = client.GetByteArrayAsync(pdfUrl).Result;

                // Open the PDF from the byte array using PdfSharp
                using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
                {
                    PdfReader pdfReader = new PdfReader(pdfStream);
                    PdfDocument pdfDoc = new PdfDocument(pdfReader);


                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                    // get all pageText form 1. page
                    string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(1), strategy);


                    // values for extracting specific text
                    List<string> indexes = new List<string> { "Atļauja Nr:", "Atļauja derīga no:", "līdz", "Nosaukums:", "Reģ.Nr.::", "Izmaiņas veiktas:", "Maršruts:", "Kustība atļauta:" };
                    List<string> results = new List<string> { };

                    // extracting specific text
                    for (int i = 0; i < indexes.Count; i++) 
                    {
                        // Find the index
                        int index = pageText.IndexOf(indexes[i]);

                        if (index != -1 && indexes[i] != "Maršruts:" && indexes[i] != "Kustība atļauta:")
                        {
                            // Get the text after index
                            string restOfString = pageText.Substring(index + indexes[i].Length);

                            // Find the index of the next newline character
                            int newlineIndex = restOfString.IndexOf("\n");

                            // Extract the line of text after the keyword and trim "/n"
                            string result = newlineIndex != -1 ? restOfString.Substring(0, newlineIndex) : restOfString.TrimEnd();

                            // declare for using it outside "if"
                            string trimmedResult;

                            // check for permitDate since
                            if (indexes[i] == "Atļauja derīga no:")
                            {
                                // get only the first word
                                string trimmed = result.Trim();
                                int spaceIndex = trimmed.IndexOf(' ');
                                string firstWord = spaceIndex != -1 ? trimmed.Substring(0, spaceIndex) : trimmed;

                                trimmedResult = firstWord;
                            }
                            else
                            {
                                // trim both ends of result
                                trimmedResult = result.Trim();
                            }

                            // Display the result
                            Console.WriteLine("Text after '" + indexes[i] + "': '" + trimmedResult + "'");
                        }

                        // FIX THIS!!!!!!!!!!!!!!!!!!!!!!!!!!
                        /*
                        else if (index != -1 && indexes[i] == "Maršruts:") {
                            // Get the substring starting from the end of the keyword
                            string restOfString = pageText.Substring(index + indexes[i].Length);

                            // Split the substring into lines
                            string[] lines = restOfString.Split("\n");

                            // Take the next two lines
                            string nextTwoLines = string.Join("\n", lines.Take(2));

                            Console.WriteLine("Text after 'Maršruts:':\n" + nextTwoLines.Trim());
                        }
                        else if (index != -1 && indexes[i] == "Kustība atļauta:") {
                            // Get the substring starting from the end of the keyword
                            string restOfString = pageText.Substring(index + indexes[i].Length);

                            // Split the substring into lines
                            string[] lines = restOfString.Split('\n');

                            // Take the next line
                            string nextLine = lines.FirstOrDefault();

                            Console.WriteLine("Text after 'Kustība atļauta:': " + nextLine.Trim());
                        }
                        */
                        else
                        {
                            Console.WriteLine("Substring not found in the input string.");
                        }
                    }

                    // write whole page
                    Console.WriteLine("/n" + pageText);
                    
                    // Close everything when done
                    pdfReader.Close();
                    pdfDoc.Close();
                    pdfStream.Close();
                }
            }
        }
    }
}