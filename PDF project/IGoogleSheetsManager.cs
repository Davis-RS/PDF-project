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
        Spreadsheet createNew(string documentName);

        void createEntry(string sheetName, string sheetId, List<object> objectList);

        ValueRange getValue(string sheetId, string valueRange);

        void removeValue(string sheetId, string valueRange);

        void updateEntry(string sheetName, string sheetId, string value);

        string getId(string sheetUrl);
    }
}
