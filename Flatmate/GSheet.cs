using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
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

        private string _doneId;
        private string _subUrbId;
        private string _flatId;

        private SheetsService _service;

        private UserCredential _credential;

        private bool _querryStatus;
        private string _log;

        public string AppName { get => _appName; set => _appName = value; }
        public string User { get => _user; set => _user = value; }
        public string SpreadSheetUrl { get => _spreadSheetUrl; set => _spreadSheetUrl = value; }

        public bool QuerryStatus { get => _querryStatus; set => _querryStatus = value; }
        public string Log { get => _log; set => _log = value; }

        public GSheet()
        {
            var text=File.ReadAllText("AppName.txt");
            AppName=text.Split(':')[1].Trim();
        }

        public void InitCredentials(string user)
        {
            QuerryStatus = false;

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
                    Log= "Timeout Error:\r\nAuthorization could not be completed within alloted time";
                }

                _credential = task.Result;

                QuerryStatus = true;
                Log= "Authorization Successfull";
            }
            catch(Exception ex)
            {
                Log = "Authorization Error:\r\n" + ex.Message;
            }
        }

        public void InitSpreadSheetService(string spreadSheetUrl)
        {
            QuerryStatus = false;

            try
            {
                _service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = AppName,
                });

                SpreadSheetUrl = spreadSheetUrl;

                GetSpreadSheetId();

                QuerryStatus = true;
                Log= "SheetsService Initalized";
            }
            catch(Exception ex)
            {
                Log= "SheetsService Error:\r\n" + ex.Message;
            }
        }

        public void InitSheets()
        {
            QuerryStatus = false;
            try
            {
                var request = _service.Spreadsheets.Get(_spreadSheetId);

                Spreadsheet resp = request.Execute();

                string doneId = null,flatId=null, subUrbId=null;

                foreach(var sheet in resp.Sheets)
                {
                    if (sheet.Properties.Title == "Done")
                    {
                        doneId = sheet.Properties.SheetId.ToString();
                    }
                    else if (sheet.Properties.Title == "Flat")
                    {
                        flatId = sheet.Properties.SheetId.ToString();
                    }
                    else if(sheet.Properties.Title== "SuburbList")
                    {
                        subUrbId = sheet.Properties.SheetId.ToString();
                    }
                }

                if (string.IsNullOrEmpty(doneId))
                    throw new Exception("Done sheet is missing from spreadsheet");

                if (string.IsNullOrEmpty(flatId))
                    throw new Exception("Flat sheet is missing from spreadsheet");

                if (string.IsNullOrEmpty(subUrbId))
                    throw new Exception("SuburbList is missing from spreadsheet");

                

                QuerryStatus = true;
                Log= "Downloaded the spreadsheet properties";
                                
            }
            catch(Exception ex)
            {
                Log = ex.Message;
            }
        }

        public void UpdateDoneFlag()
        {
            QuerryStatus = false;

            var req=_service.
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
