using System.Reflection.PortableExecutable;
using System;
using System.Net.Http;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.IO;
using System.Reflection.Metadata;
using Google.Apis.Auth.OAuth2;

namespace PDF_project
{
    class Program
    {
        static void Main()
        {
            // google api user credentials
            string googleClientId = "936433672894-6582lb3te5cg9vv5628isvos5qje1gat.apps.googleusercontent.com";
            string googleClientSecret = "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";
            string[] scopes = new[] { Google.Apis.Sheets.v4.SheetsService.Scope.Spreadsheets };

            // google sheets id
            string spreadsheetId = "1a4Zn2y-zzPDsxzBm8yyT5qWcqeyC85yS_x4VYRqyE7U";

            UserCredential credential = GoogleAuthentication.Login(googleClientId, googleClientSecret, scopes);
            GoogleSheetsManager manager = new GoogleSheetsManager(credential);

            // create new sheet
            // var newSheet = manager.CreateNew("Test document");
            // Console.WriteLine(newSheet.SpreadsheetUrl);
            
            List<object> results = new List<object> { };


            // URL of PDF file 
            //  177659 177744 177262 178317 178567
            string pdfUrl = "https://va.lvceli.lv/Request/request/Application/GetPermissionPdfFile?id=178535";

            // download PDF and get specific data from it
            using (HttpClient client = new HttpClient())
            {
                // download the PDF file as a byte array
                byte[] pdfBytes = client.GetByteArrayAsync(pdfUrl).Result;

                // open the PDF from the byte array using PdfSharp
                using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
                {
                    PdfReader pdfReader = new PdfReader(pdfStream);
                    PdfDocument pdfDoc = new PdfDocument(pdfReader);


                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                    // get all pageText form 1. page
                    string pageText = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(1), strategy);


                    // values for extracting specific text
                    List<string> indexes = new List<string> { "Atļauja Nr:", "Atļauja derīga no:", "līdz", "Nosaukums:", "Reģ.Nr.::", "Izmaiņas veiktas:", "Kustība atļauta:", "Maršruts:", "Garums:", "Platums:", "Augstums no brauktuves:", "Transportlīdzekļa faktiskā masa (ar kravu) līdz:", "Asu skaits:", "Slodze uz asi(t):", "Attālums starp asīm:" };


                    // extracting specific text
                    for (int i = 0; i < indexes.Count; i++)
                    {
                        // Find the index
                        int index = pageText.IndexOf(indexes[i]);

                        if (index != -1 && indexes[i] != "Maršruts:" && indexes[i] != "Kustība atļauta:" && indexes[i] != "Attālums starp asīm:")
                        {
                            // get the text after index
                            string restOfString = pageText.Substring(index + indexes[i].Length);

                            // find the index of the next newline character
                            int newlineIndex = restOfString.IndexOf("\n");

                            // extract the line of text after the keyword and trim "/n"
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

                            // display the result
                            Console.WriteLine("Text after '" + indexes[i] + "': '" + trimmedResult + "'");

                            // add result to results list
                            results.Add(trimmedResult);
                        }
                        else if (index != -1 && indexes[i] == "Maršruts:")
                        {
                            // get end index
                            int endIndex = pageText.IndexOf("Kustība atļauta:");

                            // get the substring between the start and end keywords
                            string substringBetween = pageText.Substring(index + indexes[i].Length, endIndex - (index + indexes[i].Length));

                            // get the substring starting from the end of the keyword
                            string trimmedResult = substringBetween.Trim();

                            // display the result
                            Console.WriteLine("Text after 'Maršruts:': '" + trimmedResult + "'");

                            // add result to results list
                            results.Add(trimmedResult);
                        }
                        else if (index != -1 && indexes[i] == "Kustība atļauta:")
                        {
                            // get end index
                            int endIndex = pageText.IndexOf("Vispārīgie nosacījumi:");

                            // get the substring between the start and end keywords
                            string substringBetween = pageText.Substring(index + indexes[i].Length, endIndex - (index + indexes[i].Length));

                            // take the next line
                            string trimmedResult = substringBetween.Trim();

                            // display the result
                            Console.WriteLine("Text after 'Kustība atļauta:': '" + trimmedResult + "'");

                            // add result to results list
                            results.Add(trimmedResult);
                        }
                        else if (index != -1 && indexes[i] == "Attālums starp asīm:")
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

                            // display the result
                            Console.WriteLine("Text after 'Attālums starp asīm:': '" + trimmedResult + "'");

                            // add result to results list
                            results.Add(trimmedResult);
                        }
                        else
                        {
                            Console.WriteLine("Substring '" + indexes[i] + "' not found in the input string.");
                            results.Add("-");
                        }
                    }

                    // write whole page
                    // Console.WriteLine(pageText);

                    // Close everything when done
                    pdfReader.Close();
                    pdfDoc.Close();
                    pdfStream.Close();
                }
            }
            // Console.Read();

            // append data to sheet
            manager.CreateEntry("Test document", spreadsheetId, results);
        }
    }
}