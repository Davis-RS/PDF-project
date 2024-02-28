using Google.Apis.Auth.OAuth2;
using Google.Apis.Http;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDF_project
{
    internal class GoogleSheetsManager : IGoogleSheetsManager
    {
        private readonly UserCredential _credential;

        public GoogleSheetsManager(UserCredential credential)
        {
            _credential = credential;
        }

        // create new spreadsheet
        public Spreadsheet createNew(string documentName)
        {
            Console.WriteLine("Creating new Google Sheet...");

            if (string.IsNullOrEmpty(documentName))
                throw new ArgumentNullException(nameof(documentName));
            
            using (var sheetsService = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = _credential})) 
            {
                var documentCreationRequest = sheetsService.Spreadsheets.Create(new Spreadsheet()
                {
                    Sheets = new List<Sheet>()
                    {
                        new Sheet()
                        {
                            Properties = new SheetProperties()
                            {
                                Title = documentName
                            }
                        }
                    },

                    Properties = new SpreadsheetProperties()
                    {
                        Title = documentName
                    }
                });

                return documentCreationRequest.Execute();
            }
        }


        // create new entry
        public void createEntry(string sheetName, string sheetId, List<object> objectList)
        {

            using (var sheetsService = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = _credential }))
            {
                // specify the range of cells
                var range = $"{sheetName}!A:O";
                var valueRange = new ValueRange();

                valueRange.Values = new List<IList<object>> { objectList };

                // create append request to be executed
                var appendRequest = sheetsService.Spreadsheets.Values.Append(valueRange, sheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

                // save response for debugging
                var appendResponse = appendRequest.Execute();
            }
        }


        // get a value from sheet
        public ValueRange getValue(string sheetId, string valueRange)
        {
            if (string.IsNullOrEmpty(sheetId))
            {
                throw new ArgumentNullException(nameof(sheetId));
            }
            if (string.IsNullOrEmpty(valueRange))
            {
                throw new ArgumentNullException(nameof(valueRange));
            }

            using (var sheetsService = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = _credential }))
            {
                var getValueRequest = sheetsService.Spreadsheets.Values.Get(sheetId, valueRange);
                return getValueRequest.Execute();
            }
        }


        // remove a value from sheet
        public void removeValue(string sheetId, string valueRange)
        {
            if (string.IsNullOrEmpty(sheetId))
            {
                throw new ArgumentNullException(nameof(sheetId));
            }
            if (!string.IsNullOrEmpty(valueRange))
            {
                throw new ArgumentNullException(nameof(valueRange));
            }

            using (var sheetsService = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = _credential }))
            {
                var removeValueRequest = sheetsService.Spreadsheets.Values.Clear(new ClearValuesRequest(), sheetId, valueRange);
                removeValueRequest.Execute();
            }
        }


        // add a value to sheet
        public void updateEntry(string sheetId, string sheetName, string value)
        {
            Console.WriteLine("Updating last id cell...");


            if (string.IsNullOrEmpty(sheetId))
            {
                throw new ArgumentNullException(nameof(sheetId));
            }
            if (!string.IsNullOrEmpty(value))
            {
                throw new ArgumentNullException(nameof(value));
            }

            using (var sheetsService = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = _credential }))
            {
                // specify the range of cells
                var range = $"{sheetName}!R2";
                var valueRange = new ValueRange();

                var objectList = new List<object>() { value };
                valueRange.Values = new List<IList<object>> { objectList };

                // create append request to be executed
                var updateRequest = sheetsService.Spreadsheets.Values.Update(valueRange, sheetId, range);
                updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;

                // save response for debugging
                var updateResponse = updateRequest.Execute();

                if (updateResponse != null)
                {
                    Console.WriteLine($"Last id ({value}) cell updated.");
                }
            }

        }


        // get spreadsheet id for further actions with sheets
        public string getId(string sheetUrl)
        {
            Console.WriteLine("Getting sheet id...");

            string id = "";
            
            int startIndex = sheetUrl.IndexOf("/d/");
            int endIndex = sheetUrl.IndexOf("/edit", startIndex + "/d/".Length);

            if (startIndex != -1 && endIndex != -1)
            {
                id = sheetUrl.Substring(startIndex + "/d/".Length, endIndex - startIndex - "/d/".Length);
            }

            return id;
        }
    }
}
