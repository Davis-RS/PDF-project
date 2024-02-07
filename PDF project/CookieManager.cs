using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace PDF_project
{
    internal class CookieManager
    {
        // variable
        private DateTime timeFrame;

        // property
        public DateTime TimeFrame
        {
            get { return timeFrame; }
            set { timeFrame = value; }
        }


        public string getCookieValueFromResponse(HttpResponseMessage response, string cookieName)
        {
            Console.WriteLine("Getting cookie value...");

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (string.IsNullOrEmpty(cookieName))
            {
                throw new ArgumentException("Cookie name cannot be null or empty.", nameof(cookieName));
            }

            string cookieValue = "";

            if (response.Headers.TryGetValues("Set-Cookie", out var setCookieHeaders))
            {
                foreach (var setCookieHeader in setCookieHeaders)
                {
                    // get only first part of header value
                    var cookies = setCookieHeader.Split(';')
                        .Select(cookie => cookie.Trim())
                        .Select(cookie => cookie.Split('='))
                        .ToDictionary(cookieParts => cookieParts[0], cookieParts => cookieParts.Length > 1 ? cookieParts[1] : "");

                    if (cookies.TryGetValue(cookieName, out var value))
                    {
                        // get cookie value
                        cookieValue = HttpUtility.UrlDecode(value);

                        // get timeframe of gathering the cookie
                        timeFrame = DateTime.Now;

                        break;
                    }
                }
            }

            return cookieValue;
        }

        public string getTokenValueFromResponse(HttpResponseMessage response, string headerName)
        {
            Console.WriteLine("Getting token value...");

            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (string.IsNullOrEmpty(headerName))
            {
                throw new ArgumentException("Cookie name cannot be null or empty.", nameof(headerName));
            }

            string headerValue = "";

            if (response.Headers.TryGetValues("Requestverificationtoken", out var values))
            {
                // get the value
                headerValue = values.First();
            }
            else
            {
                Console.WriteLine("Header value not found!");
            }

            return headerValue;
        }


        public bool verifyCookie()
        {
            Console.WriteLine("Verifying cookie...");

            // get current time
            DateTime currentTime = DateTime.Now;

            // get time difference
            TimeSpan timeDifference = currentTime - timeFrame;

            // check if 5 minutes have passed
            // bool isExpired = timeDifference.TotalSeconds >= 3;
            bool isExpired = timeDifference.TotalMinutes >= 5;

            Console.WriteLine($"Validation: Current time: {currentTime} , cookie time: {timeFrame}");

            return isExpired;
        }
    }
}
