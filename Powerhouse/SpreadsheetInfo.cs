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
        internal const string GuidRange = @"A2:A";
        internal const char GuidColumn = 'A';
        internal const string TimestampRange = @"B2:B";
        internal const char TimestampColumn = 'B';
        internal const string OffenderRange = @"C2:C";
        internal const char OffenderColumn = 'C';
        internal const string ActionRange = @"D2:D";
        internal const char ActionColumn = 'D';
        internal const string StaffRange = @"E2:E";
        internal const char StaffColumn = 'E';
        internal const string TimeRange = @"F2:F";
        internal const char TimeColumn = 'F';
        internal const string ReasonRange = @"G2:G";
        internal const char ReasonColumn = 'G';
        internal const int TotalColumns = 7;

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

        private static int ColumnToZeroBasedIndex(char c)
        {
            int index;
            switch(c)
            {
                case 'a':
                case 'A':
                    index = 0;
                    break;
                case 'b':
                case 'B':
                    index = 1;
                    break;
                case 'c':
                case 'C':
                    index = 2;
                    break;
                case 'd':
                case 'D':
                    index = 3;
                    break;
                case 'e':
                case 'E':
                    index = 4;
                    break;
                case 'f':
                case 'F':
                    index = 5;
                    break;
                case 'g':
                case 'G':
                    index = 6;
                    break;
                default:
                    index = -1;
                    break;
            }

            return index;
        }
        /// <summary>Wrapper for ColumnToZeroBasedIndex.</summary>
        internal static int CTZBI(char c) => ColumnToZeroBasedIndex(c);

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

        /// <summary>Append a new entry to the spreadsheet.</summary>
        /// <returns>Boolean value indicating success.</returns>
        internal async Task<bool> AppendEntryAsync(LogItem logItem)
        {
            var valueRange = new ValueRange(); // Value range returned by the API.
            bool success; // Was data placement successful?

            // Set the the values we want to set.
            valueRange.Values = new List<IList<object>>() { logItem.ToValueRange(logItem) };

            var appendRequest = SheetsService.Spreadsheets.Values.Append(valueRange, SpreadsheetId, Range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            var appendResponse = await appendRequest.ExecuteAsync();

            // First half of the check is checking if cells were even updated.
            if (appendResponse.Updates.UpdatedCells.HasValue && appendResponse.Updates.UpdatedCells.Value > 0)
            {
                // Second half of the check is physically investigating the cell to see if our Guid is there.
                success = await ConfirmDataPlacement(appendResponse.Updates.UpdatedRange, logItem.Guid);
            } else success = false;

            return success;
        }

        private async Task<bool> ConfirmDataPlacement(string range, string guid)
        {
            // Check if the data is there.
            bool confirmedPlaced;

            // Our value range is the entire range that was modified.
            ValueRange valueRange = await SheetsService.Spreadsheets.Values
                .Get(spreadsheetId: SpreadsheetId, range: range).ExecuteAsync();

            // Protection against out-of-bounds access.
            if (valueRange.Values.Count > 0) 
            {
                if (valueRange.Values[0].Count > 0)
                {
                    confirmedPlaced = valueRange.Values[0][0].Equals(guid);
                }
                else confirmedPlaced = false;
            }
            else confirmedPlaced = false;

            return confirmedPlaced;
        }

        internal async Task<ValueRange> QueryRange(string range)
        {
            ValueRange valueRange = await SheetsService.Spreadsheets.Values
                  .Get(spreadsheetId: SpreadsheetId, range: range).ExecuteAsync();

            return valueRange;
        }

        /// <summary>Check if a Guid exists.</summary>
        /// <returns>The cell in which the Guid resides.</returns>
        internal async Task<Cell> QueryGuidExists(string guid)
        {
            int? zeroBasedRow = null;
            Cell returnVal;

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
                returnVal = Cell.ZeroBasedRowToCell(GuidColumn, zeroBasedRow.Value);
            else
                returnVal = Cell.InvalidCell;

            return returnVal;
        }

        internal async Task<LogItem> GetLogItemAsync(int row, bool zeroBasedInteger = false)
        {
            LogItem logItem;

            string range = String.Format("{0}{1}:{2}{1}",
                    /*0*/ GuidColumn,
                    /*1*/ row + (zeroBasedInteger ? 2 : 0),
                    /*2*/ ReasonColumn);


            ValueRange valueRange = await SheetsService.Spreadsheets.Values
                .Get(spreadsheetId: SpreadsheetId, range: range).ExecuteAsync();

            // Make sure we don't go out of range.
            if (valueRange.Values.Count == 1)
            {
                // Make sure we don't go out of range.
                if (valueRange.Values[0].Count == TotalColumns)
                {
                    // All cells in the row.
                    IList<object> values = valueRange.Values[0];

                    logItem = LogItem.GenerateLogItemFromIList(values);
                } else logItem = default; // out of range, some kind of error happened
            } else logItem = default; // out of range, some kind of error happened

            return logItem;
        }
    }
}
