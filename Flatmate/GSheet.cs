using Google.Apis.Auth.OAuth2;
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

        private UserCredential _credential;

        public string AppName { get => _appName; set => _appName = value; }

        public GSheet(string user)
        {
            _user = user;
            var text=File.ReadAllText("AppName.txt");
            AppName=text.Split(':')[1].Trim();

            using(var stream=new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/sheets.googleapis.com-dotnet-flatmate.json");

                _credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    _scopes,
                    user,
                    CancellationToken.None,
                    new FileDataStore(credPath, true)
                    ).Result;
            }
        }

        private void CreateAndSaveCredentials()
        {

        }
    }
}
