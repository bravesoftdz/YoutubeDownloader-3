using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
        private List<TokenEx> _tokens = new List<TokenEx>();
        public static bool hasPcToken = false;

        public TokenService()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{App.Name} ({App.GitHubProjectUrl})");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YoutubeDownloader (github.com/derech1e/YoutubeDownloader)");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "token 448d39603553439c25adb24e11ed666bb5724e17");
        }

        public async Task CacheJson()
        {
            if (!_tokens.IsNullOrEmpty()) return;
            var response = await TryGetTokenJsonAsync();
            var tokens = JsonConvert.DeserializeObject<List<TokenEx>>(response!);
            _tokens!.AddRange(tokens);
        }

        public async Task<bool?> IsTokenVaild(string tokenFromInput)
        {
            await CacheJson();
            foreach (TokenEx tokenEx in _tokens)
            {
                if (tokenEx.Token!.Equals(tokenFromInput.Trim()))
                    if (hasPcToken)
                        return (bool)tokenEx.Activated!;
                    else return (bool)tokenEx.Activated! && !(bool)tokenEx.Used!;
            }

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
