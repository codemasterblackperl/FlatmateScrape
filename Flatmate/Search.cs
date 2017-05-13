using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flatmate
{
    class Search
    {
        private readonly string _url="https://flatmates.com.au";
        private readonly string _userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";

        private HttpHelper _client;

        public Search()
        {
            _client = new HttpHelper();
            _client.InitHttpClient(true,_userAgent,"flatmates.com.au", "https://flatmates.com.au/");
        }

        async public Task DownloadWebPage(string keyword)
        {
            var html = await _client.GetAsync(_url);
            File.WriteAllText(@"c:\temp\f.htm", html);

            var authToken = GetToken(html);
        }

        public string GetToken(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var div = doc.DocumentNode.SelectSingleNode("//div[@data-react-class='Header']");
            var content = div.GetAttributeValue("data-react-props", "");
            content = HtmlEntity.DeEntitize(content);

            dynamic data = JObject.Parse(content);

            dynamic session =JObject.Parse(data.session.ToString());

            var token = session.csrf.token.ToString();

            return token;
            //var token = doc.DocumentNode.SelectSingleNode("//input[@name='authenticity_token']");
            //return token.GetAttributeValue("content", "");
        }


        private string GetRoomSearch(string keyword,string authToken)
        {

            //authenticity_token = IZlOXRusFSIipRhoZM6pE1a96Cuk644m6fMF0mHSRkfQgqLL4Ep1RKjvHf % 2BuSCByDfs2XgM94NhrNBQZHgUgtA % 3D % 3D 
            //& search % 5Blocations % 5D % 5B % 5D = cherrybrook - 2126 
            //& search % 5Bmode % 5D = rooms 
            //& search % 5Bmin_budget % 5D = 
            //&search % 5Bmax_budget % 5D = 
            //&search % 5Bsort % 5D = photos

            var token = HtmlEntity.Entitize(authToken);
            string postString = "authenticity_token=" + token;
            return postString;
        }
    }
}
