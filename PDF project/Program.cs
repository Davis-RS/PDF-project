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

                    // excract data from each page
                    for (int page = 1; page <= pdfDoc.GetNumberOfPages(); page++)
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();

                        string data = PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(page), strategy);

                        Console.WriteLine(data);
                    }

                    Console.WriteLine("Wow šis actually strādā :D");

                    // Close everything when done
                    pdfReader.Close();
                    pdfDoc.Close();
                    pdfStream.Close();
                }
            }
        }
    }
}