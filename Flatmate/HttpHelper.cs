using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flatmate
{
    public class HttpHeader
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
    class HttpHelper
    {
        private HttpClient _client;
        private CookieContainer _cookieContainer;

        public string ResponseUri { get; set; }

        public bool AllowRedirection { get; set; }
        public string UserAgent { get; set; }
        public string Host { get; set; }
        public string Referer { get; set; }
        public string Accept { get; set; }

        public CookieContainer CookieContainer { get => _cookieContainer; set => _cookieContainer = value; }

        public readonly static string _acceptCommon = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        public readonly static string _acceptJson = "application/json";



        public void InitHttpClient(List<HttpHeader> customHeaders=null, WebProxy proxy = null)
        {
            if(CookieContainer==null)
                CookieContainer = new CookieContainer();

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = AllowRedirection,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = CookieContainer,
                UseCookies = true
            };

            if (proxy != null)
            {
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }

            ServicePointManager.Expect100Continue = false;

            _client = new HttpClient(handler);


            _client.DefaultRequestHeaders.Add("user-agent", UserAgent);
            _client.DefaultRequestHeaders.Add("Accept", Accept);
            _client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.8");
            _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

            _client.DefaultRequestHeaders.Add("Connection", "keep-alive");

            _client.DefaultRequestHeaders.Add("Host", Host);

            if (Referer != string.Empty)
                _client.DefaultRequestHeaders.Add("Referer", Referer);

            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                    _client.DefaultRequestHeaders.Add(header.Name, header.Value);
            }
        }


        public async Task<string> GetAsync(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            ResponseUri = response.RequestMessage.RequestUri.ToString();
            return await response.Content.ReadAsStringAsync();
        }

        async public Task<string> PostAsync(string url, HttpContent content)
        {
            var resp = await _client.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }
    }
}
