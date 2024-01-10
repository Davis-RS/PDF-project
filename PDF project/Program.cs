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
        // CookieManager instance
        static CookieManager cookieManager = new CookieManager();

        static async Task Main()
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


            // GoogleManager instance
            UserCredential credential = GoogleAuthentication.Login(googleClientId, googleClientSecret, scopes);
            GoogleSheetsManager sheetsManager = new GoogleSheetsManager(credential);

            // PdfManager instance
            PdfManager pdfManager = new PdfManager();
            
            string cookieValue;

            // create new sheet
            // var newSheet = sheetsManager.CreateNew("Test document");
            //Console.WriteLine(newSheet.SpreadsheetUrl);


            // stopwatch to see how much ms to execute pdf processing
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();

            //---------------------------------------------------------------------------------------------------------------------------------------

            for (var i = 0; i < 10; i++)
            {
                if (cookieManager.verifyCookie())
                {
                    Console.WriteLine("Cookie is not valid!");
                    cookieValue = await getCookieValueAsync("https://va.lvceli.lv/Request/Permission/index", ".AspNetCore.Antiforgery.Bh1b6bYpeVU");
                }
                else
                {
                    Console.WriteLine("Cookie is valid!");
                }
                Thread.Sleep(1000);
            }

            // downloading pdf and getting results
            pdfManager.getResults(pdfUrl);


            // Console.Read();
            //Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

            // append data to sheet
            //sheetsManager.CreateEntry("Test document", spreadsheetId, pdfManager.Results);
        }


        static async Task<string> getCookieValueAsync(string url, string cookieName)
        {

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {

                    // call function that returns cookie value
                    string cookieValue = cookieManager.getCookieValueFromResponse(response, cookieName);
                    Console.WriteLine("getCookieValueFromResponse is called.");

                    Console.WriteLine($"Timeframe: {cookieManager.TimeFrame}");
                    Console.WriteLine($"Cookie value: {cookieValue}");


                    return cookieValue;
                }
                else
                {
                    return "Error while getting HttpResponse in getCookieValueAsync";
                }
            }

        }
    }
}