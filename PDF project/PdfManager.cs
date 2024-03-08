using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Kernel.Pdf.Canvas.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Net.Http;
using System.Reflection.Metadata;

namespace PDF_project
{
    internal class PdfManager
    {
        // variable
        private List<string> resultsFailed = new List<string>();
        private List<object> results = new List<object>();
        private string pageText = string.Empty;
        
        // key words for extracting specific text
        private List<string> indexes = new List<string>()
        { 
            "Atļaujas Nr:", 
            "derīga no: ", 
            "līdz ", 
            "Nosaukums:", 
            "Reģ.Nr.:", 
            "veiktas:", 
            "Kustība atļauta:", 
            "Maršruts:", 
            "Garums:", 
            "Platums:", 
            "Augstums no braukt", 
            "Transportlīdzekļa faktiskā masa", 
            "Asu skaits:", 
            "Slodze uz asi(t):", 
            "Attālums starp asīm:" 
        };


        // property
        public List<string> ResultsFailed
        {
            get { return resultsFailed; }
            set { resultsFailed = value; }
        }

        public List<object> Results
        {
            get { return results; }
            set { results = value; }
        }

        public string PageText
        {
            get { return pageText; }
            set { pageText = value; }
        }


        // check if extracting essential values was succesful
        bool CheckFirstFiveValues(List<object> list)
        {
            for (int i = 1; i < 8; i++)
            {
                if (list[i] == "-")
                {
                    return false;
                }
            }

            return true;
        }


        // download pdf file
        private void getPageText(HttpClient client, string pdfUrl, bool writePageText)
        {
            Console.WriteLine("Downloading PDF file...");

            pageText = "";

            // download the PDF file as a byte array
            byte[] pdfBytes = client.GetByteArrayAsync(pdfUrl).Result;

            // save pdf
            using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
            {
                // open the PDF from the byte array using PdfSharp
                PdfReader pdfReader = new PdfReader(pdfStream);
                PdfDocument pdfDoc = new PdfDocument(pdfReader);

                // for extracting text
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                // get all pageText form 1. page
                pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(1), strategy);


                if (writePageText)
                {
                    // write whole page
                    Console.WriteLine(pageText);
                }


                // close everything when done
                pdfReader.Close();
                pdfDoc.Close();
                pdfStream.Close();
            }
        }

        public List<object> getResults(HttpClient client, string pdfUrl, bool writePageText, bool debugMode)
        {
            // get page text
            getPageText(client, pdfUrl, writePageText);

            Console.WriteLine("Getting results from PDF file...");

            // stopwatch to see how much ms to execute pdf processing
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            // empty result list
            results.Clear();

            // extracting specific text
            for (int i = 0; i < indexes.Count; i++)
            {
                // find the index
                int index = pageText.IndexOf(indexes[i]);

                if (index != -1 && i == 1) // derīga no
                {
                    string result = pageText.Substring(index + indexes[i].Length, 10);

                    if (debugMode)
                    {
                        // display the result
                        Console.WriteLine($"Text after '{indexes[i]}': '{result}'");
                    }

                    // add result to results list
                    results.Add(result);
                }
                else if (index != -1 && i == 6) // kustība atļauta:
                {
                    // get end index
                    int endIndex = pageText.IndexOf("Vispārīgie nosacījumi:");

                    // get the substring between the start and end keywords
                    string substringBetween = pageText.Substring(index + indexes[i].Length, endIndex - (index + indexes[i].Length));

                    // take the next line
                    string trimmedResult = substringBetween.Trim();
                    
                    if (debugMode)
                    {
                        // display the result
                        Console.WriteLine($"Text after '{indexes[i]}': '{trimmedResult}'");
                    }
                    
                    // add result to results list
                    results.Add(trimmedResult);
                }
                else if (index != -1 && i == 7) // maršruts
                {
                    // get end index
                    int endIndex = pageText.IndexOf("Kustība atļauta:");

                    // get the substring between the start and end keywords
                    string substringBetween = pageText.Substring(index + indexes[i].Length, endIndex - (index + indexes[i].Length));

                    // get the substring starting from the end of the keyword
                    string trimmedResult = substringBetween.Trim();

                    if (debugMode)
                    {
                        // display the result
                        Console.WriteLine($"Text after '{indexes[i]}': '{trimmedResult}'");
                    }

                    // add result to results list
                    results.Add(trimmedResult);
                }
                else if (index != -1 && i == 8) // garums
                {
                    // get end index
                    int endIndex = pageText.IndexOf(indexes[10]);

                    // get the substring between the start and end keywords
                    string substringBetween = pageText.Substring(index + indexes[i].Length, endIndex - (index + indexes[i].Length));

                    // get the substring starting from the end of the keyword
                    string trimmedResult = substringBetween.Trim();

                    if (debugMode)
                    {
                        // display the result
                        Console.WriteLine($"Text after '{indexes[i]}': '{trimmedResult}'");
                    }

                    // add result to results list
                    results.Add(trimmedResult);
                }
                else if (index != -1 && i == 9) // platums
                {
                    // get end index
                    int endIndex = pageText.IndexOf(indexes[11]);

                    // get the substring between the start and end keywords
                    string substringBetween = pageText.Substring(index + indexes[i].Length, endIndex - (index + indexes[i].Length));

                    // get the substring starting from the end of the keyword
                    string trimmedResult = substringBetween.Trim();

                    if (debugMode)
                    {
                        // display the result
                        Console.WriteLine($"Text after '{indexes[i]}': '{trimmedResult}'");
                    }

                    // add result to results list
                    results.Add(trimmedResult);
                }
                else if (index != -1 && i == 10) // augstums no brauktuves
                {
                    int endIndex = pageText.IndexOf(indexes[11]);

                    // get the substring between the start and end keywords
                    string substringBetween = pageText.Substring(index + indexes[i].Length + 7, 7);

                    // get the substring starting from the end of the keyword
                    string trimmedResult = substringBetween.Trim();

                    if (debugMode)
                    {
                        // display the result
                        Console.WriteLine($"Text after '{indexes[i]}': '{trimmedResult}'");
                    }

                    // add result to results list
                    results.Add(trimmedResult);
                }
                else if (index != -1 && i == 11) // transportlidzekla faktiska masa
                {
                    string result = pageText.Substring(index + indexes[i].Length + 19, 7);

                    if (debugMode)
                    {
                        // display the result
                        Console.WriteLine($"Text after '{indexes[i]}': '{result}'");
                    }

                    // add result to results list
                    results.Add(result);
                }
                else if (index != -1 && i == 14) // attālums starp asīm
                {
                    // get end index
                    int endIndex = pageText.IndexOf("Maršruts:");

                    // get the substring between the start and end keywords
                    string substringBetween = pageText.Substring(index + indexes[i].Length, endIndex - (index + indexes[i].Length));

                    // get the substring starting from the end of the keyword
                    string trimmedResult = substringBetween.Trim();

                    // get rid of every anomaly in result
                    trimmedResult = trimmedResult.Replace("\n", "");
                    trimmedResult = trimmedResult.Replace("⎿", "");
                    trimmedResult = trimmedResult.Replace("⊥", "");
                    trimmedResult = trimmedResult.Replace("⏌", "");
                    trimmedResult = trimmedResult.Trim();

                    if (debugMode)
                    {
                        // display the result
                        Console.WriteLine($"Text after '{indexes[i]}': '{trimmedResult}'");
                    }

                    // add result to results list
                    results.Add(trimmedResult);
                }
                else if (index != -1)
                {
                    // get the text after index
                    string restOfString = pageText.Substring(index + indexes[i].Length);

                    // find the index of the next newline character
                    int newlineIndex = restOfString.IndexOf("\n");

                    // extract the line of text after the keyword and trim "/n"
                    string result = newlineIndex != -1 ? restOfString.Substring(0, newlineIndex) : restOfString.TrimEnd();

                    // declare for using it outside "if"
                    string trimmedResult;

                    // trim both ends of result
                    trimmedResult = result.Trim();

                    if (debugMode)
                    {
                        // display the result
                        Console.WriteLine($"Text after '{indexes[i]}': '{trimmedResult}'");
                    }

                    // add result to results list
                    results.Add(trimmedResult);

                }
                else
                {
                    if (debugMode)
                    {
                        Console.WriteLine($"Substring '{indexes[i]}' not found in the input string.");
                    }
                    
                    results.Add("-");
                }
            }

            Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

            if (CheckFirstFiveValues(results))
            {
                Console.WriteLine("Extraction successful!");
            }
            else
            {
                Console.WriteLine("Ectration wasn't successful!");
                resultsFailed.Add(pdfUrl);
            }

            return results;
        }        
    }
}
