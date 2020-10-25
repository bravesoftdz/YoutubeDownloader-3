using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using YoutubeDownloader.Internal;

namespace YoutubeDownloader.Services
{
    public class TokenService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private List<TokenEx> _tokens = new List<TokenEx>();

        public TokenService()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{App.Name} ({App.GitHubProjectUrl})");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YoutubeDownloader (github.com/derech1e/YoutubeDownloader)");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "token 448d39603553439c25adb24e11ed666bb5724e17");
        }

        public void CacheJson()
        {
            var response = TryGetTokenJsonAsync();
            var tokens = JsonConvert.DeserializeObject(response!, typeof(List<TokenEx>)) as List<TokenEx>;
            _tokens.AddRange(tokens!);
        }

        public bool IsTokenVaild(string token)
        {
            CacheJson();
            foreach (TokenEx item in _tokens!)
            {
                bool vaild = item.token.Equals(token.Trim()) && item.activated && !item.used;
                return vaild;
            }

            return false;
        }

        private string TryGetTokenJsonAsync()
        {
            try
            {
                var url = Uri.EscapeUriString("https://raw.githubusercontent.com/derech1e/YoutubeDownloader/master/tokens.json");

                using var response = _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, new CancellationTokenSource().Token);

                if (!response.Result.IsSuccessStatusCode)
                    return "";

                var raw = response.Result.Content.ReadAsStringAsync();
                return raw.Result;
            }
            catch
            {
                return "";
            }
        }
    }
}
