using System;
using System.Collections.Generic;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Powerhouse
{
    internal class SpreadsheetInfo
    {
        private const string ApplicationName = "Project Powerhouse";
        internal const string InvalidRange = "#INVALIDRANGE";
        const string GuidRange = @"A2:A";
        const char GuidRow = 'A';

        internal SheetsService SheetsService;
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        internal string SpreadsheetId;
        internal string Range;
        
        
        internal SpreadsheetInfo() { }
        internal SpreadsheetInfo(GoogleCredential userCredential, string spreadsheetId, string range)
        {
            SheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = ApplicationName,
            });

            SpreadsheetId = spreadsheetId;
            Range = range;
        }

        internal SpreadsheetInfo(string file, string spreadsheetId, string range)
            : this(userCredential: GetUserCredentials(file),
                   spreadsheetId: spreadsheetId,
                   range: range) { }

        private static GoogleCredential GetUserCredentials(string file)
        {
            GoogleCredential userCredential;

            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                userCredential = GoogleCredential.FromStream(fs)
                    .CreateScoped(Scopes);
            }

            return userCredential;
        }

        private string ZeroBasedRowToCellId(char column, int index)
        {
            return column + (index + 2).ToString();
        }

        internal async Task<bool> AppendEntryAsync(LogItem logItem, string range)
        {
            var valueRange = new ValueRange();

            valueRange.Values = new List<IList<object>>() { logItem.ToValueRange(logItem) };

            var appendRequest = SheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            var appendResponse = await appendRequest.ExecuteAsync();

            return appendResponse.Updates.UpdatedCells.HasValue && appendResponse.Updates.UpdatedCells.Value > 0;
        }

        

        internal async Task<string> QueryGuidExists(string guid)
        {
            int? zeroBasedRow = null;
            string returnVal;

            ValueRange valueRange = await SheetsService.Spreadsheets.Values
                  .Get(spreadsheetId: SpreadsheetId, range: GuidRange).ExecuteAsync();

            if (valueRange.Values.Count > 0)
            {
                bool continueLoop = true;
                int i = 0;
                foreach(string foundGuid in valueRange.Values.Select(a => a[0]))
                {
                    if (continueLoop)
                    {
                        if (foundGuid.Equals(guid))
                        {
                            zeroBasedRow = i;
                            continueLoop = false;
                        }
                    }
                    else break; // NON-SESE ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! !

                    i++;
                }
            }

            if (zeroBasedRow.HasValue)
                returnVal = ZeroBasedRowToCellId(GuidRow, zeroBasedRow.Value);
            else
                returnVal = InvalidRange;

            return returnVal;
        }
    }
}
