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
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Util.Store;

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
            // PDF exctraction options
            bool outputEachPdfText = false;
            bool outputExtractedResults = false;

            // emailing properties
            // Gmail API user credentials
            string fromMail = "4davisroberts@gmail.com";
            string fromPassword = "xwsbxqkefxvzolfr";

            // list of emails to send messages to
            var toEmails = new List<string>()
            {
                "4davisroberts@gmail.com"
            };


            // google sheet properties
            // google api user credentials
            string googleClientId = "936433672894-6582lb3te5cg9vv5628isvos5qje1gat.apps.googleusercontent.com";
            string googleClientSecret = "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";

            // google sheet url
            string sheetUrl = "https://docs.google.com/spreadsheets/d/1oBN75A_OSInwUhWrInb9HpbJLKzKfciCczp9NfhPmMY/edit#gid=1458323830";

            // sheet tab name
            string sheetName = "Atlaujas";


            // google sheets id - leave blank
            string sheetId = string.Empty;

            // site values
            string requestUrl = "https://va.lvceli.lv/Request/request";
            string cookieValue = null;
            string verificationTokenValue = string.Empty;
            string jsonResponse = string.Empty;

            // JSON values
            int itemCount = 0; // keep 0

            // number of items in one JSON page
            int pagesize = 200;


            // new Google Sheets service
            string[] scopes = new[] { Google.Apis.Sheets.v4.SheetsService.Scope.Spreadsheets };


            // Google credentials - old version
            //UserCredential credential = GoogleAuthentication.Login(googleClientId, googleClientSecret, scopes);
            
            // get Google credentials (hopefully working after token expiration...)
            UserCredential credential = null;

            try
            {
                var clientSecrets = new ClientSecrets
                {
                    ClientId = googleClientId,
                    ClientSecret = googleClientSecret
                };

                using (var stream = new FileStream("token.json", FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        clientSecrets,
                        scopes,
                        "user",
                        CancellationToken.None,
                        new FileDataStore("Books.ListMyLibrary"));
                }

                Console.WriteLine("Google authorization successful!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Google authorization failed: {ex.Message}");
            }

            // google sheets manager instance
            GoogleSheetsManager sheetsManager = new GoogleSheetsManager(credential);

            // PdfManager instance
            PdfManager pdfManager = new PdfManager();

            // JsonManager instance
            JsonManager jsonManager = new JsonManager();


            // get sheet id
            sheetId = sheetsManager.getId(sheetUrl);
            Console.Out.WriteLine($"Sheet id: {sheetId}");

            // get Google sheet last ID
            string sheetLastID = ExtractStringFromValueRange(sheetsManager.getValue(sheetId, "LastID!B1"));
            Console.Out.WriteLine($"LastID value from Google sheet: {sheetLastID}");


            //---------------------------------------------------------------------------------------------------------------------------------------



            // cookie and verification token verification
            if (cookieManager.verifyCookie())
            {
                Console.WriteLine("Cookie is not valid!");

                if (await getCookieValueSuccessAsync(requestUrl, ".AspNetCore.Antiforgery.Bh1b6bYpeVU"))
                {
                    cookieValue = cookieManager.CookieValue;
                }
                else
                {
                    Console.Out.WriteLine("Error while getting cookieValue.");
                }

                verificationTokenValue = await getVerificationTokenValueAsync(requestUrl);

                // print values
                Console.WriteLine($"Timeframe: {cookieManager.TimeFrame}");
                Console.WriteLine($"Cookie value: {cookieManager.CookieValue}");
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

            // get current last ID from jsonResponse
            string currentLastID = jsonManager.getCurrentLastID(jsonResponse);
            Console.WriteLine($"LastID value from va.lvceli.lv: {currentLastID}");


            // number of loops to complete whole 
            double loops = (int)Math.Ceiling((double)itemCount / pagesize);
            Console.Out.WriteLine($"No of loops: {loops}");

            bool isUpdated = false;

            // loops of 200 ids
            for (int i = 0; i < loops; i++)
            {
                if (isUpdated)
                {
                    Console.Out.WriteLine("The list has been updated.");
                    break;
                }

                // cookie and verification token verification
                if (cookieManager.verifyCookie())
                {
                    Console.WriteLine("Cookie is not valid!");

                    client = new HttpClient();

                    if(await getCookieValueSuccessAsync(requestUrl, ".AspNetCore.Antiforgery.Bh1b6bYpeVU"))
                    {
                        cookieValue = cookieManager.CookieValue;
                    }
                    else
                    {
                        Console.Out.WriteLine("Error while getting cookieValue.");
                    }
                    
                    verificationTokenValue = await getVerificationTokenValueAsync(requestUrl);

                    // print values
                    Console.WriteLine($"Timeframe: {cookieManager.TimeFrame}");
                    Console.WriteLine($"Cookie value: {cookieManager.CookieValue}");
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
                    // check if 
                    if (int.Parse(id) <= int.Parse(sheetLastID))
                    {
                        isUpdated = true;
                        break;
                    }

                    Console.WriteLine($"PDF id: {id}");

                    string pdfUrl = $"https://va.lvceli.lv/Request/request/Application/GetPermissionPdfFile?id={id}";

                    // download pdf and get results
                    pdfManager.getResults(client, pdfUrl, outputEachPdfText, outputExtractedResults);

                    // append data to sheet
                    sheetsManager.createEntry(sheetName, sheetId, pdfManager.Results);
                }
            }

            // update LastID entry for future updates
            sheetsManager.updateEntry(sheetId, "LastID", currentLastID);

            // notify for failed results
            Console.WriteLine($"PDF document exctraction failed: {pdfManager.ResultsFailed}");

            // send emails to every user
            EmailManager emailManager = new EmailManager();
            emailManager.sendEmails(toEmails, fromMail, fromPassword, sheetUrl);
            
            
        }


        public static UserCredential RefreshToken(string googleClientId, string googleClientSecret, string refreshToken, string[] scopes)
        {
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = googleClientId,
                    ClientSecret = googleClientSecret
                },
                Scopes = scopes
            });

            TokenResponse tokenResponse = flow.RefreshTokenAsync("user", refreshToken, CancellationToken.None).Result;

            // Create and return a new UserCredential with the refreshed token
            return new UserCredential(flow, "user", tokenResponse);
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


        static async Task<bool> getCookieValueSuccessAsync(string url, string cookieName)
        {
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("getCookieValueFromResponse is called.");

                return cookieManager.getCookieValueFromResponse(response, cookieName);
            }
            else
            {
                Console.WriteLine("Error while getting HttpResponse in getCookieValueAsync");
                return false;
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

        static string ExtractStringFromValueRange(ValueRange valueRange)
        {
            // Extract the char from the ValueRange
            // For example, assuming the ValueRange contains a single cell with a character
            if (valueRange.Values != null && valueRange.Values.Count > 0 && valueRange.Values[0].Count > 0)
            {
                string cellValue = valueRange.Values[0][0]?.ToString();

                if (!string.IsNullOrEmpty(cellValue))
                {
                    return cellValue; // Assuming the first character of the cell value
                }
            }

            // Handle the case where the ValueRange doesn't contain the expected data
            throw new InvalidOperationException("Invalid or empty ValueRange");
        }
    }
}