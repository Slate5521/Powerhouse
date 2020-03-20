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
            string reason, long tickDuration = -1)
        {
            LogItem logItem = new LogItem(
                seconds: time.ToUnixTimeSeconds(),
                offenderId: offenderId,
                staffmemberId: staffmemberId,
                action: action,
                duration: tickDuration,
                reason: reason);

            return await spreadsheetInfo.AppendEntryAsync(logItem);
        }

        #endregion Adding Entries
        #region Updating/Deleting Entries

        public async Task<bool> UpdateEntry(string guid, long? tickDuration, string reason = @"")
        {
            return await spreadsheetInfo.UpdateEntry(guid, tickDuration, reason);
        }

        public async Task<bool> DeleteEntry(string guid)
        {
            return await spreadsheetInfo.DeleteRow(guid);
        }

        #endregion Updating/Deleting Entries
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

        public async Task<LogItem[]> QueryPendingActions()
        {
            List<LogItem> pendingList = new List<LogItem>(); //new Dictionary<long, string>();
            ValueRange timerangeQuery = await spreadsheetInfo
                .QueryRange(SpreadsheetInfo.TimeRange);
            LogItem[] returnVal;

            // Pull from every timerange value.
            for(int i = 0; i < timerangeQuery.Values.Count; i++)
            {
                string cellContents;
                if (timerangeQuery.Values[i].Count > 0)
                    cellContents = timerangeQuery.Values[i].Single<object>().ToString();
                else cellContents = String.Empty;

                // We're going to pull every time-limited entry we find.
                if(cellContents.Length > 1 && 
                    !cellContents.Substring(1).Equals("0") &&
                    !cellContents.Substring(1).Equals("-1"))
                {
                    Cell currTimerangeCell =
                        Cell.ZeroBasedRowToCell(SpreadsheetInfo.TimeColumn, i);

                    // Gets all the entries.
                    ValueRange fullRowQuery = await spreadsheetInfo
                        .QueryRange(String.Format("{0}{1}:{2}{1}",
                                                /*0*/ SpreadsheetInfo.GuidColumn,
                                                /*1*/ currTimerangeCell.Row,
                                                /*2*/ SpreadsheetInfo.ReasonColumn));

                    // Only continue if there's data to gather.
                    if(fullRowQuery.Values.Count > 0)
                    {
                        long curTicksMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                        foreach (IList<object> obj in fullRowQuery.Values)
                        {
                            LogItem logItem = LogItem.GenerateLogItemFromIList(obj);

                            if (logItem.Equals(default))
                                // We have an issue
                                throw new Exception("Please verify data.");
                            else
                            {
                                if(logItem.Timed)
                                        pendingList.Add(logItem);
                            }
                        }
                    }
                }
            }

            long currTicks = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (pendingList.Count > 0)
                returnVal = pendingList.OrderBy(a => a.RemainingTicks(currTicks)).ToArray();
            else returnVal = default;

            return returnVal;
        }

        public async Task<LogItem[]> QueryPendingActions(ulong offenderId)
        {
            LogItem[] logItems = await QueryPendingActions();

            if (logItems.Count() > 0)
                return logItems.Where(a => a.OffenderId == offenderId).ToArray();
            else return default;
        }

        #endregion
    }
}
