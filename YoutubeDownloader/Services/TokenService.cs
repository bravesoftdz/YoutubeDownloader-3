using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Tyrrrz.Extensions;
using YoutubeDownloader.Internal;

namespace YoutubeDownloader.Services
{
    public class TokenService
    {
        private readonly HttpClient _httpClient = new HttpClient();
        private Dictionary<string, bool> tokenCache;

        public TokenService()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{App.Name} ({App.GitHubProjectUrl})");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YoutubeDownloader (github.com/derech1e/YoutubeDownloader)");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "token 448d39603553439c25adb24e11ed666bb5724e17");
            tokenCache = new Dictionary<string, bool>();
        }

        public async Task<bool?> IsTokenVaild(string token)
        {
            if(!tokenCache.IsNullOrEmpty() && tokenCache != null)
                if(tokenCache.ContainsKey(token))
                    return tokenCache.GetValueOrDefault(token);
                
            var response = await TryGetTokenJsonAsync();
            var tokens = JsonConvert.DeserializeObject(response!, typeof(List<TokenEx>)) as List<TokenEx>;

            foreach (TokenEx item in tokens!)
            {
                if (item.token.Equals(token.Trim()) && item.activated && !item.used)
                {
                    if (tokenCache.ContainsKey(token))
                        tokenCache.Remove(token);

                    tokenCache.Add(token, true);
                    return true;
                }
            }
            if (tokenCache.ContainsKey(token))
                tokenCache.Remove(token);

            tokenCache.Add(token, false);
            return false;
        }

        private async Task<string?> TryGetTokenJsonAsync()
        {
            try
            {
                var url = Uri.EscapeUriString("https://raw.githubusercontent.com/derech1e/YoutubeDownloader/master/tokens.json");

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, new CancellationTokenSource().Token);

                if (!response.IsSuccessStatusCode)
                    return null;

                var raw = await response.Content.ReadAsStringAsync();
                return raw;
            }
            catch
            {
                return null;
            }
        }
    }
}
