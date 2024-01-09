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


            // URL of PDF file 
            //  177659 177744 177262 178317 178567    
            string pdfid = "179366";
            string pdfUrl = $"https://va.lvceli.lv/Request/request/Application/GetPermissionPdfFile?id={pdfid}";


            // GoogleManager instances
            UserCredential credential = GoogleAuthentication.Login(googleClientId, googleClientSecret, scopes);
            GoogleSheetsManager sheetsManager = new GoogleSheetsManager(credential);

            // PdfManager instance
            PdfManager pdfManager = new PdfManager();


            // create new sheet
            // var newSheet = sheetsManager.CreateNew("Test document");
            //Console.WriteLine(newSheet.SpreadsheetUrl);


            // stopwatch to see how much ms to execute pdf processing
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();


            pdfManager.getResults(pdfUrl);

            Console.WriteLine(pdfManager.Results);


            // Console.Read();
            //Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

            // append data to sheet
            sheetsManager.CreateEntry("Test document", spreadsheetId, pdfManager.Results);
        }
    }
}