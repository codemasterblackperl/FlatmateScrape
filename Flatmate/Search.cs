using HtmlAgilityPack;
using Newtonsoft.Json;
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

        private int _pageCount;

        private string _authToken;
        private string _csrfToken;

        public Search()
        {
            //_client = new HttpHelper();
            //_client.InitHttpClient(true,_userAgent,"flatmates.com.au", "https://flatmates.com.au/");
            _pageCount = 0;
        }

        async public Task InitFlatmateSearch()
        {
            _client = new HttpHelper
            {
                AllowRedirection = true,
                UserAgent = _userAgent,
                Host = "flatmates.com.au",
                Referer = "https://flatmates.com.au/"
            };
            _client.InitHttpClient();

            var html = await _client.GetAsync(_url);
            File.WriteAllText(@"c:\temp\f" + _pageCount + ".htm", html);
            _pageCount++;

            //get the token from the html
            _authToken = GetToken(html);
            _csrfToken = GetCsrfToken(html);
        }

        async public Task<string> SearchFlat(string suburb,string state)
        {
            
            

            // get the location details
            var postCode=await GetLocation(suburb,state);

            var keyword = suburb + "-" + postCode;

            await Task.Delay(2000);

            //build search querry
            var queery = BuildRoomSearchQuerry(keyword, _authToken);


            var html = await _client.PostAsync("https://flatmates.com.au/searches", queery);

            File.WriteAllText(@"c:\temp\f" + _pageCount + ".htm", html);
            _pageCount++;

            return html;
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

        public string GetCsrfToken(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var div = doc.DocumentNode.SelectSingleNode("//meta[@name='csrf-token']");
            var content = div.GetAttributeValue("content", "");
            //content = HtmlEntity.DeEntitize(content);

            //dynamic data = JObject.Parse(content);

            //dynamic session = JObject.Parse(data.session.ToString());

            //var token = session.csrf.token.ToString();

            return content;
        }

        async public Task<string> GetLocation(string suburb,string state)
        {
            var loc = suburb + " " + state;
            var client= new HttpHelper
            {
                Accept=HttpHelper._acceptJson,
                CookieContainer=_client.CookieContainer,
                AllowRedirection = true,
                UserAgent = _userAgent,
                Host = "flatmates.com.au",
                Referer = "https://flatmates.com.au/"
            };

            var customHeaders = new List<HttpHeader>
            {
                new HttpHeader { Name = "X-CSRF-Token", Value = _csrfToken },
                new HttpHeader{Name="Origin",Value="https://flatmates.com.au"}
                //new HttpHeader { Name = "Content-Type", Value = "application/json;charset=UTF-8" }
            };
            client.InitHttpClient(customHeaders);

            System.Net.Http.StringContent postString = new System.Net.Http.StringContent(
                "{\"location_suggest\":{\"text\":\""+loc+"\",\"completion\":{\"field\"" +
                ":\"suggest\",\"size\":5,\"fuzzy\":{\"fuzziness\":\"AUTO\"},\"contexts\":" +
                "{\"location_type\":[\"suburb\",\"city\",\"university\",\"tram_stop\",\"train_station\"]}}}}",Encoding.UTF8,
                "application/json");
            var locReceived = await client.PostAsync("https://flatmates.com.au/autocomplete", postString);

            var postalCode = GetPostalCodeFromJson(locReceived);

            return postalCode;
        }

        private string GetPostalCodeFromJson(string jsonString)
        {
            try
            {
                //jsonString = "{\"took\":20,\"timed_out\":false,\"_shards\":{\"total\":5,\"successful\":5,\"failed\":0},\"hits\":{\"total\":0,\"max_score\":0.0,\"hits\":[]},\"suggest\":{\"location_suggest\":[{\"text\":\"cherrybrook nsw\",\"offset\":0,\"length\":15,\"options\":[{\"text\":\"Cherrybrook NSW\",\"_index\":\"locations_production_20170321151001639\",\"_type\":\"location\",\"_id\":\"523\",\"_score\":210.0,\"_source\":{\"id\":523,\"state\":\"NSW\",\"city\":\"Sydney\",\"suburb\":\"Cherrybrook\",\"postcode\":\"2126\",\"country\":\"AU\",\"created_at\":\"2014-12-29T14:53:38.612Z\",\"updated_at\":\"2017-05-18T14:01:47.462Z\",\"latitude\":-33.7287467,\"longitude\":151.0413133,\"polygon\":[],\"location_type\":\"suburb\",\"key\":\"cherrybrook-2126\",\"average_rent\":238,\"temp_latitude\":null,\"temp_longitude\":null,\"radius\":6,\"name\":null,\"short_name\":null,\"synonyms\":[],\"location\":[151.0413133,-33.7287467],\"search_title\":\"Cherrybrook, Sydney, NSW, 2126\",\"short_title\":\"Cherrybrook, 2126\",\"suggest\":{\"input\":[\"Sydney\",\"2126\",\"Cherrybrook\",\"Cherrybrook 2126\",\"Cherrybrook NSW\"],\"weight\":14,\"contexts\":{\"location_type\":[\"suburb\"]}}},\"contexts\":{\"location_type\":[\"suburb\"]}}]}]}}";

                var json = JsonConvert.DeserializeObject<JsonLocation>(jsonString);

                var postalCode = json.suggest.location_suggest[0].options[0]._source.postcode;

                return postalCode;
            }
            catch
            {
                throw new Exception("Error when parsing location data received");
            }
            //dynamic json = JsonConvert.DeserializeObject(jsonString);

            //var locString =json.suggest.location_suggest.ToString();
            //locString = locString.TrimStart('{');
            //locString = locString.TrimEnd('}');

            //dynamic loc = JsonConvert.DeserializeObject(locString);
            //dynamic data = JObject.Parse(jsonString);

            //dynamic suggest = JObject.Parse(data.suggest.ToString());

            //dynamic locationSuggest = JObject.Parse(suggest.location_suggest.ToString());
        }


        private System.Net.Http.FormUrlEncodedContent BuildRoomSearchQuerry(string keyword,string authToken)
        {

            //authenticity_token = IZlOXRusFSIipRhoZM6pE1a96Cuk644m6fMF0mHSRkfQgqLL4Ep1RKjvHf % 2BuSCByDfs2XgM94NhrNBQZHgUgtA % 3D % 3D 
            //& search % 5Blocations % 5D % 5B % 5D = cherrybrook - 2126 
            //& search % 5Bmode % 5D = rooms 
            //& search % 5Bmin_budget % 5D = 
            //&search % 5Bmax_budget % 5D = 
            //&search % 5Bsort % 5D = photos

            var querry = new System.Net.Http.FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("authenticity_token",authToken),
                new KeyValuePair<string, string>("search[locations][]",keyword),
                new KeyValuePair<string, string>("search[mode]","rooms"),
                new KeyValuePair<string, string>("search[min_budget]",""),
                new KeyValuePair<string, string>("search[max_budget]",""),
                new KeyValuePair<string, string>("search[sort]","photos")
            });

            //var keys = new KeyValuePair<string, string>();
            //keys.ad

            //var token = HtmlEntity.Entitize(authToken);
            //string postString = "authenticity_token=" + token;
            //postString += "&search%5Blocations%5D%5B%5D=" + keyword;
            //postString += "&search%5Bmode%5D=rooms&search%5Bmin_budget%5D=&search%5Bmax_budget%5D=&search%5Bsort%5D=photos";
            //return postString;

            return querry;
        }

        public List<Flat> GetFlatDetails(string html,string suburb)
        {
            var list = new List<Flat>();

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            //var nodes = doc.DocumentNode.SelectNodes("//div[@class='content-column']");
            var nodes = doc.DocumentNode.SelectNodes("//div[@data-reactid='20']/div");

            if (nodes == null || nodes.Count == 0)
                throw new Exception("No result found");

            foreach(var node in nodes)
            {
                var pnode = node.SelectSingleNode("./div/a[@class='hero']/div");
                var price = pnode.InnerText.Trim();

                var nnode = node.SelectSingleNode("./div/h2");
                var name = nnode.InnerText.Trim();

                list.Add(new Flat
                {
                    Price=price,
                    Name=name,
                    SubUrb=suburb,
                    RoomOrFlatmate="Room"
                });

            }

            return list;
        }
    }
}
