using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Internal;

namespace YoutubeDownloader.Services
{
    public class TokenService
    {
        private readonly HttpClient _httpClient = new HttpClient();

        public TokenService()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{App.Name} ({App.GitHubProjectUrl})");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YoutubeDownloader (github.com/derech1e/YoutubeDownloader)");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "token 448d39603553439c25adb24e11ed666bb5724e17");
        }

        public async Task<bool?> IsTokenVaild(string token, CancellationToken cancellationToken)
        {
            var response = await TryGetTokenJsonAsync(cancellationToken);
            var tokens = JsonConvert.DeserializeObject(response!, typeof(List<TokenEx>)) as List<TokenEx>;

            foreach (TokenEx item in tokens!)
            {
                if (item.token == token && item.activated == true) return true;
            }

            return false;
        }

        private async Task<string?> TryGetTokenJsonAsync(CancellationToken cancellationToken)
        {
            try
            {
                var url = Uri.EscapeUriString("https://raw.githubusercontent.com/derech1e/YoutubeDownloader/master/tokens.json");

                using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

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
