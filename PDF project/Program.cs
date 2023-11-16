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
            //  177659 177744
            string pdfUrl = "https://va.lvceli.lv/Request/request/Application/GetPermissionPdfFile?id=177262";

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

                            // add result to results list
                            results.Add(trimmedResult);
                        }
                        else if (index != -1 && indexes[i] == "Maršruts:") {
                            // get end index
                            int endIndex = pageText.IndexOf("Kustība atļauta:");

                            // Get the substring between the start and end keywords
                            string substringBetween = pageText.Substring(index + indexes[i].Length, endIndex - (index + indexes[i].Length));

                            // Get the substring starting from the end of the keyword
                            string trimmedResult = substringBetween.Trim();

                            Console.WriteLine("Text after 'Maršruts:': '" + trimmedResult + "'");

                            // add result to results list
                            results.Add(trimmedResult);
                        }
                        else if (index != -1 && indexes[i] == "Kustība atļauta:") {
                            // get end index
                            int endIndex = pageText.IndexOf("Vispārīgie nosacījumi:");

                            // Get the substring between the start and end keywords
                            string substringBetween = pageText.Substring(index + indexes[i].Length, endIndex - (index + indexes[i].Length));

                            // Take the next line
                            string trimmedResult = substringBetween.Trim();

                            Console.WriteLine("Text after 'Kustība atļauta:': '" + trimmedResult + "'");
                            
                            // add result to results list
                            results.Add(trimmedResult);
                        }
                        else
                        {
                            Console.WriteLine("Substring not found in the input string.");
                        }
                    }

                    // FOR TESTING
                    // write whole page
                    // Console.WriteLine("/n" + pageText);
                    
                    // Close everything when done
                    pdfReader.Close();
                    pdfDoc.Close();
                    pdfStream.Close();
                }
            }
        }
    }
}