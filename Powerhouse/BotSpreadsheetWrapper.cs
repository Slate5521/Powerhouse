using Google.Apis.Sheets.v4.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Powerhouse
{
    public class BotSpreadsheetWrapper
    {
        private SpreadsheetInfo spreadsheetInfo;

        public BotSpreadsheetWrapper() { }
        public BotSpreadsheetWrapper(string credFile, string spreadsheetId, string range)
        {
            spreadsheetInfo = new SpreadsheetInfo(credFile, spreadsheetId, range);
        }


        #region Adding Entries

        /// <summary>Wrapper method to add a warn.</summary>
        public async Task<bool> AddWarn(DateTimeOffset time, ulong offenderId, ulong staffmemberId,
            string reason) => await AddAction(time, offenderId, ActionTaken.Warn, staffmemberId, reason);
        /// <summary>Wrapper method to add a mute.</summary>
        public async Task<bool> AddMute(DateTimeOffset time, ulong offenderId, ulong staffmemberId,
            string reason, long tickDuration = -1) => await AddAction(time, offenderId, ActionTaken.Mute, staffmemberId, reason, tickDuration);
        /// <summary>Wrapper method to add a kick.</summary>
        public async Task<bool> AddKick(DateTimeOffset time, ulong offenderId, ulong staffmemberId,
            string reason) => await AddAction(time, offenderId, ActionTaken.Kick, staffmemberId, reason);
        /// <summary>Wrapper method to add a ban.</summary>
        public async Task<bool> AddBan(DateTimeOffset time, ulong offenderId, ulong staffmemberId,
            string reason, long tickDuration = -1) => await AddAction(time, offenderId, ActionTaken.Ban, staffmemberId, reason, tickDuration);

        /// <summary>All wrapper methods point to this one.</summary>
        private async Task<bool> AddAction(DateTimeOffset time, ulong offenderId, ActionTaken action, ulong staffmemberId,
            string reason, long tickDuration = 0)
        {
            LogItem logItem = new LogItem(
                ticks: time.ToUnixTimeMilliseconds(),
                offenderId: offenderId,
                staffmemberId: staffmemberId,
                action: action,
                duration: tickDuration,
                reason: reason);

            return await spreadsheetInfo.AppendEntryAsync(logItem);
        }

        #endregion Adding Entries
        #region Updating Entries

        //public async Task<bool> UpdateEntry(string guid, long? tickDuration, string reason = @"")
        //{
        //    
        //}

        #endregion
        #region Queries

        /// <summary>Returns the cell of the specified Guid, if found.</summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<Cell> QueryGuid(string guid)
        {
            return await spreadsheetInfo.QueryGuidExists(guid);
        }

        public async Task<LogItem> QueryLogItemByGuidCell(Cell guidCell)
        {
            return await spreadsheetInfo.GetLogItemAsync(guidCell.Row);
        }

        public async Task<LogItem[]> QueryLogItemByOffenderId(ulong offenderId)
        {
            ValueRange offenderColumn = await spreadsheetInfo.QueryRange(SpreadsheetInfo.OffenderRange);
            List<LogItem> logItems = new List<LogItem>();

            for (int i = 0; i < offenderColumn.Values.Count; i++)
            {
                string cellContents;
                if (offenderColumn.Values[i].Count > 0)
                    cellContents = offenderColumn.Values[i].Single<object>().ToString();
                else cellContents = String.Empty;

                if(cellContents.Length > 1 && cellContents.Substring(1).Equals(offenderId.ToString()))
                    logItems.Add(await spreadsheetInfo.GetLogItemAsync(i, true));
            }

            return logItems.ToArray();
        }

        public async Task<Dictionary<long, string>> QueryTimestampGuidDictionary()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
