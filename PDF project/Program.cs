using System.Reflection.PortableExecutable;
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
using testing;
using System.Text.Json;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;
using iText.Kernel.Pdf.Action;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Sheets.v4.Data;

namespace PDF_project
{
    class Program
    {
        // CookieManager instance
        static CookieManager cookieManager = new CookieManager();

        // HttpClient instance for using all in one
        static HttpClient client = new HttpClient();

        static async Task Main()
        {
            // Gmail user credentials
            string fromMail = "4davisroberts@gmail.com";
            string fromPassword = "xwsbxqkefxvzolfr";

            // google api user credentials
            string googleClientId = "936433672894-6582lb3te5cg9vv5628isvos5qje1gat.apps.googleusercontent.com";
            string googleClientSecret = "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";
            
            // google sheets id
            string sheetId = "";
                   
            // spreadsheet name as current date and time
            // get the current date and time
            DateTime currentDateTime = DateTime.Now;

            // format the date and time as a string
            string sheetName = currentDateTime.ToString("yyyy.MM.dd HH-mm");

            // site values
            string requestUrl = "https://va.lvceli.lv/Request/request";
            string cookieValue = "";
            string verificationTokenValue = "";
            string jsonResponse = "";

            // JSON values
            int itemCount = 0;


            var sheetHeaders = new List<object>()
            {
                "Atļaujas Nr.",
                "Atļauja derīga no:",
                "Atļauja derīga līdz:",
                "Pārvadātāja nosaukums:",
                "Pārvadātāja reģ.nr.:",
                "Izmaiņas veiktas:",
                "Kustība atļauta:",
                "Maršruts:",
                "Transportlīdzekļa garums:",
                "Transportlīdzekļa platums:",
                "Transportlīdzekļa augstums no brauktuves:",
                "Transportlīdzekļa kopējā faktiskā masa:",
                "Asu skaits:",
                "Slodze uz asi(t):",
                "Attālums starp asīm:"
            };

            var toEmails = new List<string>()
            {
                "4davisroberts@gmail.com"
            };

            // new Google Sheets service
            string[] scopes = new[] { Google.Apis.Sheets.v4.SheetsService.Scope.Spreadsheets };


            // GoogleManager instance
            UserCredential credential = GoogleAuthentication.Login(googleClientId, googleClientSecret, scopes);
            GoogleSheetsManager sheetsManager = new GoogleSheetsManager(credential);

            // PdfManager instance
            PdfManager pdfManager = new PdfManager();

            // JsonManager instance
            JsonManager jsonManager = new JsonManager();

            Spreadsheet newSheet = null;

            // create new sheet
            try
            {
                newSheet = sheetsManager.CreateNew(sheetName);
            }
            catch (TokenResponseException ex)
            {
                Console.Out.WriteLine($"Token error while creating spreadsheet: {ex.Message}");
                Console.Out.WriteLine($"Trying again to get Google auth screen.");

                // try again to get google auth popup
                newSheet = sheetsManager.CreateNew(sheetName);
            }
            catch (Exception ex)
            {
                // Handle other exceptions if needed
                Console.WriteLine($"Error while creating spreadsheet: {ex.Message}");
            }

            // get all sheet info
            string sheetUrl = newSheet.SpreadsheetUrl;
            Console.WriteLine($"New sheet name: {sheetName} sheet url: {sheetUrl}");

            // get sheet id
            string newId = sheetsManager.getId(sheetUrl);
            sheetId = newId;
            Console.Out.WriteLine($"Sheet id: {sheetId}");
                        
            // fill sheet with headers
            sheetsManager.CreateEntry(sheetName, sheetId, sheetHeaders);
            

            //---------------------------------------------------------------------------------------------------------------------------------------

            // cookie and verification token verification
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

            // get max count of items
            try
            {
                jsonResponse = await SendPostRequest(cookieValue, 1, 1, verificationTokenValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            // get max count of PDF files
            if (itemCount == 0)
            {
                itemCount = jsonManager.getMaxCount(jsonResponse);
            }

            // number of items in one page
            int pagesize = 200;

            // number of loops to complete 
            double loops = (int)Math.Ceiling((double)itemCount / pagesize);
            Console.Out.WriteLine($"No of loops: {loops}");
            
            for (int i = 0; i < loops; i++)
            {
                // cookie and verification token verification
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


                // send POST request
                jsonResponse = await SendPostRequest(cookieValue, i+1, pagesize, verificationTokenValue);
                Console.WriteLine(jsonResponse);

                // get all ids from json response
                List<string> ids = jsonManager.getResults(jsonResponse, "\"id\":\"", "\",\"");

                // loop through each id
                foreach (string id in ids)
                {
                    Console.WriteLine($"PDF id: {id}");

                    string pdfUrl = $"https://va.lvceli.lv/Request/request/Application/GetPermissionPdfFile?id={id}";

                    // download pdf and get results
                    pdfManager.getResults(client, pdfUrl, false, true);

                    // append data to sheet
                    sheetsManager.CreateEntry(sheetName, sheetId, pdfManager.Results);
                }
            }

            // send emails to every user
            EmailManager emailManager = new EmailManager();
            emailManager.sendEmails(toEmails, fromMail, fromPassword, sheetUrl);
        }


        static async Task<string> SendPostRequest(string cookieValue, int page, int pagesize, string tokenValue)
        {
            // URL for the POST request
            string url = "https://va.lvceli.lv/Request/request";

            // JSON payload
            var postData = new PostData()
            {
                page = page,
                pagesize = pagesize,
            };

            // create a request message with method, URL, and content
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                // add headers, including the custom cookie
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


                // convert to json
                var jsonPayload = JsonSerializer.Serialize(postData);

                // add JSON payload to the request content
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                // send the request and get the response
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    // ensure a successful response
                    response.EnsureSuccessStatusCode();

                    // read the response content as a string
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


        // function that returns verification token value for sending request
        static async Task<string> getVerificationTokenValueAsync(string url)
        {
            // send a GET request to the specified URL
            HttpResponseMessage response = await client.GetAsync(url);

            // ensure a successful response
            response.EnsureSuccessStatusCode();

            // read the response content as a string
            string htmlContent = await response.Content.ReadAsStringAsync();

            // use HtmlAgilityPack to parse the HTML
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // find the RequestVerificationToken in the HTML
            HtmlNode tokenNode = htmlDocument.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']");

            // get and print token
            var value = tokenNode?.Attributes["value"]?.Value;
            await Console.Out.WriteLineAsync($"Verification token value: {value}");

            // return the value of the RequestVerificationToken if found
            return value;
        }
    }
}