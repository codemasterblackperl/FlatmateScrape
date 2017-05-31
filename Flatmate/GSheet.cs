using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flatmate
{
    class GSheet
    {
        private string[] _scopes = { SheetsService.Scope.Spreadsheets };
        private string _appName;
        private string _user;
        private string _spreadSheetUrl;

        private string _spreadSheetId;

        private UserCredential _credential;

        public string AppName { get => _appName; set => _appName = value; }
        public string User { get => _user; set => _user = value; }
        public string SpreadSheetUrl { get => _spreadSheetUrl; set => _spreadSheetUrl = value; }

        public GSheet()
        {
            var text=File.ReadAllText("AppName.txt");
            AppName=text.Split(':')[1].Trim();
        }

        public string InitCredentials(string user)
        {
            User = user;
            ClientSecrets secret;

            using (var stream = new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                secret = GoogleClientSecrets.Load(stream).Secrets;
            }

            string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-flatmate.json");
            try
            {
                var task = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secret,
                    _scopes,
                    user,
                    CancellationToken.None,
                    new FileDataStore(credPath, true)
                    );

                bool result = task.Wait(60000 * 2);

                if (!result)
                {
                    return "Timeout Error:\r\nAuthorization could not be completed within alloted time";
                }

                _credential = task.Result;

                return "Authorization Successfull";
            }
            catch(Exception ex)
            {
                return "Authorization Error:\r\n"+ex.Message;
            }
        }

        public string InitSpreadSheetService(string spreadSheetUrl)
        {
            try
            {
                var service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = AppName,
                });

                SpreadSheetUrl = spreadSheetUrl;

                GetSpreadSheetId();

                return "SheetsService Initalized";
            }
            catch(Exception ex)
            {
                return "SheetsService Error:\r\n" + ex.Message;
            }
        }

        private void GetSpreadSheetId()
        {
            //https://docs.google.com/spreadsheets/d/1_gsxzdnqU3Nbh-z20lUEg3S0ht07l1XZnygE-rI6k2c/edit?ts=590d2674#gid=763988035

            var tUrl = SpreadSheetUrl.Replace("https://docs.google.com/spreadsheets/d/", "");
            var data = tUrl.Split('/');
            _spreadSheetId = data[0];

        }
    }
}
