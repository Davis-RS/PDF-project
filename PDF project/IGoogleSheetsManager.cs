using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4.Data;

namespace PDF_project
{
    internal interface IGoogleSheetsManager
    {
        Spreadsheet CreateNew(string documentName);

        void CreateEntry(string sheetName, string spreadsheetId, List<object> objectList);
    }
}
