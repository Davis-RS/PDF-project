﻿using System.Reflection.PortableExecutable;
using System;
using System.Net.Http;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using System.IO;
using System.Reflection.Metadata;
using Google.Apis.Auth.OAuth2;
using System.Text;
using HtmlAgilityPack;

namespace PDF_project
{
    class Program
    {
        // CookieManager instance
        static CookieManager cookieManager = new CookieManager();

        static HttpClient client = new HttpClient();

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

            string requestUrl = "https://va.lvceli.lv/Request/request";
            string cookieValue = "";
            string verificationTokenValue = "";

            // create new sheet
            // var newSheet = sheetsManager.CreateNew("Test document");
            //Console.WriteLine(newSheet.SpreadsheetUrl);


            // stopwatch to see how much ms to execute pdf processing
            //var watch = new System.Diagnostics.Stopwatch();
            //watch.Start();

            //---------------------------------------------------------------------------------------------------------------------------------------

            for (var i = 0; i < 1; i++)
            {
                if (cookieManager.verifyCookie())
                {
                    Console.WriteLine("Cookie is not valid!");
                    cookieValue = await getCookieValueAsync(requestUrl, ".AspNetCore.Antiforgery.Bh1b6bYpeVU");
                    verificationTokenValue = await getVerificationTokenValueAsync(requestUrl);
                }
                else
                {
                    Console.WriteLine("Cookie is valid!");
                }
                Thread.Sleep(1000);
            }

            try
            {
                string jsonResponse = await SendPostRequest(cookieValue, verificationTokenValue);
                Console.WriteLine(jsonResponse);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // downloading pdf and getting results
            //pdfManager.getResults(pdfUrl);


            // Console.Read();
            //Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

            // append data to sheet
            //sheetsManager.CreateEntry("Test document", spreadsheetId, pdfManager.Results);
        }


        static async Task<string> SendPostRequest(string cookieValue, string tokenValue)
        {
            // URL for the POST request
            string url = "https://va.lvceli.lv/Request/request";

            // JSON payload
            string jsonPayload = "{\r\n  \"page\": 1,\r\n  \"pagesize\": 10,\r\n  \"NationalRegistrationNumber\": \"\",\r\n  \"ApplicationTypeDDL\": {\r\n    \"SelectedId\": \"\"\r\n  },\r\n  \"From\": \"\",\r\n  \"To\": \"\",\r\n  \"OrderField\": {}\r\n}";


            // Create a request message with method, URL, and content
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                // Add headers, including the custom cookie
                request.Headers.Add("authority", "va.lvceli.lv");
                request.Headers.Add("method", "POST");
                request.Headers.Add("path", "/Request/request");
                request.Headers.Add("scheme", "https");
                request.Headers.Add("Accept", "application/json, text/javascript, */*; q=0.01");
                request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
                request.Headers.Add("Accept-Language", "lv-LV,lv;q=0.9,en-US;q=0.8,en;q=0.7");
                request.Headers.Add("Content-Lenght", "129");
                //request.Headers.Add("Content-Type", "application/json; charset=UTF-8");
                request.Headers.Add("Cookie", $".AspNetCore.Antiforgery.Bh1b6bYpeVU={cookieValue}; .AspNet.Consent=yes");
                request.Headers.Add("Origin", "https://va.lvceli.lv");
                request.Headers.Add("Referer", "https://va.lvceli.lv/Request/Permission/index");
                request.Headers.Add("Requestverificationtoken", tokenValue);
                request.Headers.Add("Sec-Ch-Ua", "\"Not_A Brand\";v=\"8\", \"Chromium\";v=\"120\", \"Google Chrome\";v=\"120\"");
                request.Headers.Add("Sec-Ch-Ua-Mobile", "?0");
                request.Headers.Add("Sec-Ch-Ua-Platform", "\"Windows\"");
                request.Headers.Add("Sec-Fetch-Dest", "empty");
                request.Headers.Add("Sec-Fetch-Mode", "cors");
                request.Headers.Add("Sec-Fetch-Site", "same-origin");
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");


                // Add JSON payload to the request content
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // Send the request and get the response
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    // Ensure a successful response
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string (assuming it's JSON)
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        static async Task<string> getCookieValueAsync(string url, string cookieName)
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


        static async Task<string> getVerificationTokenValueAsync(string url)
        {
            // Send a GET request to the specified URL
            HttpResponseMessage response = await client.GetAsync(url);

            // Ensure a successful response
            response.EnsureSuccessStatusCode();

            // Read the response content as a string
            string htmlContent = await response.Content.ReadAsStringAsync();

            // Use HtmlAgilityPack to parse the HTML
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // Find the RequestVerificationToken in the HTML
            HtmlNode tokenNode = htmlDocument.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']");

            var value = tokenNode?.Attributes["value"]?.Value;
            await Console.Out.WriteLineAsync($"Verification token value: {value}");

            // Return the value of the RequestVerificationToken if found
            return value;
        }
    }
}