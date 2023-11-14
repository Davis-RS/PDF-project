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

                    /*
                    // excract data from each page
                    for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                    {
                    }
                    */

                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                    // get data form 1. page
                    string data = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(1), strategy);

                    List<string> indexes = new List<string> { "Atļauja Nr:", "Atļauja derīga no:", "līdz"};
                    List<int> lenghts = new List<int> { 11, 10, 10 };

                    // Find the index of "permit Nr: "
                    int index = data.IndexOf("Atļauja Nr:");


                    if (index != -1)
                    {
                        // Get the substring starting from the index after "permit Nr: "
                        string result = data.Substring(index + "Atļauja Nr:".Length + 1, 11);

                        Console.WriteLine("Atļaujas nr: " + result);
                    }
                    else
                    {
                        Console.WriteLine("Substring not found in the input string.");
                    }

                    Console.WriteLine(data);
                    
                    

                    // Close everything when done
                    pdfReader.Close();
                    pdfDoc.Close();
                    pdfStream.Close();
                }
            }
        }
    }
}