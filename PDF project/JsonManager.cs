using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
        public List<string> getResults(string jsonResponse, string startPattern, string endPattern)
        {
            // clear all entries from the list
            ids.Clear();


            string pattern = $"{Regex.Escape(startPattern)}(.*?){Regex.Escape(endPattern)}";
            
            MatchCollection matches = Regex.Matches(jsonResponse, pattern);

            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    // Group 1 contains the value between the patterns
                    string extractedValue = match.Groups[1].Value;
                    ids.Add(extractedValue);
                }
            }

            return ids;
        }

        public int getMaxCount(string jsonResponse)
        {
            int startIndex = jsonResponse.IndexOf("\"maxCount\":");

            string result = jsonResponse.Substring(startIndex + 11, 4);

            int.TryParse(result, out int maxCount);

            Console.WriteLine($"Total amount of PDF docs: {maxCount}");

            return maxCount;
        }
    }
}
