using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iText.StyledXmlParser.Jsoup.Select.Evaluator;

namespace PDF_project
{
    internal class JsonManager
    {
        // variable
        private List<string> ids = new List<string>();

        // property
        public List<string> Ids
        {
            get { return ids; }
            set { ids = value; }
        }


        // read json to return list of id's
        public List<string> getResults(string jsonResponse)
        {
            try
            {
                // Parse the JSON string
                JObject jsonObject = JObject.Parse(jsonResponse);

                // Check if "results" array exists
                if (jsonObject.TryGetValue("results", out JToken resultsToken) && resultsToken is JArray resultsArray)
                {
                    // Iterate through each result and get the "id" value
                    foreach (JToken result in resultsArray)
                    {
                        string id = result.Value<string>("id");
                        ids.Add(id);
                    }
                }
                else
                {
                    Console.WriteLine("No 'results' array found in the JSON response.");
                    // Handle the absence of the 'results' array as needed
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing JSON: {ex.Message}");
                // Handle the exception as needed
            }

            return ids;
        }
    }
}
