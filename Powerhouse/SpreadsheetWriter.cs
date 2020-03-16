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

namespace Powerhouse
{
    public static class SpreadsheetWriter
    {
        public static async Task<bool> CreateEntryAsync(this SpreadsheetInfo spreadsheetInfo, LogItem logItem, string range)
        {
            var valueRange = new ValueRange();

            valueRange.Values = new List<IList<object>>() { ToValueRange(logItem) };

            var appendRequest = spreadsheetInfo.SheetsService.Spreadsheets.Values.Append(valueRange, spreadsheetInfo.SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;

            var appendResponse = await appendRequest.ExecuteAsync();

            return appendResponse.Updates.UpdatedCells.HasValue && appendResponse.Updates.UpdatedCells.Value > 0;
        }

        private static List<object> ToValueRange(LogItem logItem)
        {
            return new List<object>
            {
                Guid.NewGuid().ToString("N"),
                logItem.Timestamp,
                '"' + logItem.OffenderId.ToString() + '"',
                logItem.Action.ToString(),
                '"' + logItem.StaffmemberId.ToString() + '"',
                logItem.Timed ? ('"' + logItem.TimeEnd.ToString() + '"') : "\"n/a\"",
                logItem.Reason,
            };
        }
    }
}
