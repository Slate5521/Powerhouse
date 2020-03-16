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

namespace Powerhouse
{
    public class SpreadsheetInfo
    {
        internal SheetsService SheetsService;
        private const string ApplicationName = "Project Powerhouse";
        static string[] Scopes = { SheetsService.Scope.Spreadsheets };
        internal string SpreadsheetId;

        SpreadsheetInfo() { }
        public SpreadsheetInfo(GoogleCredential userCredential, string spreadsheetId, string range)
        {
            SheetsService = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = userCredential,
                ApplicationName = ApplicationName,
            });

            SpreadsheetId = spreadsheetId;
        }

        public SpreadsheetInfo(string file, string spreadsheetId, string range)
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
    }
}
