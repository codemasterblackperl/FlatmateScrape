using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flatmate
{
    class HttpHelper
    {
        HttpClient _client;
        CookieContainer _cookieContainer;

        public string ResponseUri { get; set; }

        public void InitHttpClient(bool allowRedirection, string userAgent, string host, string referer = "", WebProxy proxy = null)
        {
            _cookieContainer = new CookieContainer();

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = allowRedirection,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                CookieContainer = _cookieContainer,
                UseCookies = true
            };

            if (proxy != null)
            {
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }

            ServicePointManager.Expect100Continue = false;

            _client = new HttpClient(handler);


            _client.DefaultRequestHeaders.Add("user-agent", userAgent);

            _client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            _client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            _client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");

            _client.DefaultRequestHeaders.Add("Connection", "keep-alive");

            _client.DefaultRequestHeaders.Add("Host", host);

            if (referer != string.Empty)
                _client.DefaultRequestHeaders.Add("Referer", referer);
        }


        public async Task<string> GetAsync(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            ResponseUri = response.RequestMessage.RequestUri.ToString();
            return await response.Content.ReadAsStringAsync();
        }

        async public Task<string> PostAsync(string url, FormUrlEncodedContent content)
        {
            var resp = await _client.PostAsync(url, content);
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadAsStringAsync();
        }
    }
}
