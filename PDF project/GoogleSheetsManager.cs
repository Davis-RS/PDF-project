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
        public Spreadsheet CreateNew(string documentName)
        {
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
        public void CreateEntry(string sheetName, string spreadsheetId, List<object> objectList)
        {
            using (var sheetsService = new SheetsService(new BaseClientService.Initializer() { HttpClientInitializer = _credential }))
            {
                // specify the range of cells
                var range = $"{sheetName}!A:H";
                var valueRange = new ValueRange();

                valueRange.Values = new List<IList<object>> { objectList };

                // create append request to be executed
                var appendRequest = sheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
                appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

                // save response for debugging
                var appendResponse = appendRequest.Execute();
            }
        }
    }
}
