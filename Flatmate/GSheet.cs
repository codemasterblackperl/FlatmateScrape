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
    public class GSheetResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

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

        public string AppName { get => _appName; set => _appName = value; }
        public string User { get => _user; set => _user = value; }
        public string SpreadSheetUrl { get => _spreadSheetUrl; set => _spreadSheetUrl = value; }

        public GSheet()
        {
            var text=File.ReadAllText("AppName.txt");
            AppName=text.Split(':')[1].Trim();
        }

        public GSheetResult InitCredentials(string user)
        {
            var gresult = new GSheetResult()
            {
                Success = false
            };
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
                    gresult.Message= "Timeout Error:\r\nAuthorization could not be completed within alloted time";
                    return gresult;
                }

                _credential = task.Result;

                gresult.Success = true;
                gresult.Message= "Authorization Successfull";
                return gresult;
            }
            catch(Exception ex)
            {
                gresult.Message = "Authorization Error:\r\n" + ex.Message;
                return gresult;
            }
        }

        public GSheetResult InitSpreadSheetService(string spreadSheetUrl)
        {
            var gresult = new GSheetResult()
            {
                Success = false
            };

            try
            {
                _service = new SheetsService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = _credential,
                    ApplicationName = AppName,
                });

                SpreadSheetUrl = spreadSheetUrl;

                GetSpreadSheetId();

                gresult.Success = true;
                gresult.Message= "SheetsService Initalized";
                return gresult;
            }
            catch(Exception ex)
            {
                gresult.Message= "SheetsService Error:\r\n" + ex.Message;
                return gresult;
            }
        }

        public GSheetResult InitSheets()
        {
            var gresult = new GSheetResult()
            {
                Success = false
            };

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

                gresult.Success = true;
                gresult.Message= "Downloaded the spreadsheet properties";
                return gresult;

                
            }
            catch(Exception ex)
            {
                gresult.Message = ex.Message;
                return gresult;
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
