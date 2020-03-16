using System;
using System.Collections.Generic;
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

        public static string InvalidRange
        {
            get => SpreadsheetInfo.InvalidRange;
        }

        #region Adding Entries

        /// <summary>Wrapper method to add a warn.</summary>
        public async Task AddWarn(DateTimeOffset time, ulong offenderId, ActionTaken action, ulong staffmemberId,
            string reason) => await AddAction(time, offenderId, action, staffmemberId, reason);
        /// <summary>Wrapper method to add a mute.</summary>
        public async Task AddMute(DateTimeOffset time, ulong offenderId, ActionTaken action, ulong staffmemberId,
            string reason, long tickDuration = 0) => await AddAction(time, offenderId, action, staffmemberId, reason, tickDuration);
        /// <summary>Wrapper method to add a kick.</summary>
        public async Task AddKick(DateTimeOffset time, ulong offenderId, ActionTaken action, ulong staffmemberId,
            string reason, long tickDuration = 0) => await AddAction(time, offenderId, action, staffmemberId, reason, tickDuration);
        /// <summary>Wrapper method to add a ban.</summary>
        public async Task AddBan(DateTimeOffset time, ulong offenderId, ActionTaken action, ulong staffmemberId,
            string reason, long tickDuration = 0) => await AddAction(time, offenderId, action, staffmemberId, reason, tickDuration);

        /// <summary>All wrapper methods point to this one.</summary>
        private async Task AddAction(DateTimeOffset time, ulong offenderId, ActionTaken action, ulong staffmemberId,
            string reason, long tickDuration = 0)
        {
            LogItem logItem = new LogItem()
            {
                Ticks = time.ToUnixTimeMilliseconds(),
                OffenderId = offenderId,
                Action = action,
                StaffmemberId = staffmemberId,
                Reason = reason,
                TimeEnd = time.AddMilliseconds(tickDuration).ToUnixTimeMilliseconds()
            };
        }

        #endregion Adding Entries
        #region Updating Entries

        //public async Task<bool> UpdateEntry(string guid, long? tickDuration, string reason = @"")
        //{
        //    
        //}

        #endregion
        #region Queries

        public async Task<string> QueryGuid(string guid)
        {
            return await spreadsheetInfo.QueryGuidExists(guid);
        }

        #endregion
    }
}
