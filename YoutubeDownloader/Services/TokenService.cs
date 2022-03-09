using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YoutubeDownloader.Language;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Token;
using YoutubeDownloader.Utils.Token.HWID;

namespace YoutubeDownloader.Services
{
    public class TokenService
    {
        private record RequestModel(string? Hwid = default, int? VideoDownloads = default,
            long? VideoDownloadLength = default);
        private record ResponseModel(int? youtubeCode = -1, bool success = false);


        public async Task<bool> IsTokenValid(string? token)
        {
            var bodyJson = JsonConvert.SerializeObject(new RequestModel(HWIDGenerator.UID));
            var bodyData = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            var response = await Http.Client.PostAsync(
                "https://europe-west1-logbookbackend.cloudfunctions.net/api/youtube/" + token, bodyData);
            var result = await response.Content.ReadAsStringAsync();

            var responseModel = new ResponseModel(-1, true);

            if (!bool.TryParse(result, out _))
            {
                responseModel = JsonConvert.DeserializeObject<ResponseModel>(result);
            }

            if (!responseModel!.success)
            {
                throw responseModel.youtubeCode switch
                {
                    0 => new TokenException(Resources.TokenVerifyView_Invaild_Ex),
                    1 => new TokenException(Resources.TokenVerifyView_Disabled_Ex),
                    2 => new TokenException(Resources.TokenVerifyView_Expired_Ex),
                    3 => new TokenException(Resources.TokenVerifyView_Amount_Ex),
                    _ => new TokenException("-1")
                };
            }

            return responseModel.success;
        }

        public bool UpdateStats(SettingsService settingsService)
        {
            var bodyJson = JsonConvert.SerializeObject(new RequestModel(HWIDGenerator.UID,
                settingsService.VideoDownloads, settingsService.VideoDownloadsLength));
            var bodyData = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            using var client = Http.Client;
            var response = client.PatchAsync(
                "https://europe-west1-logbookbackend.cloudfunctions.net/api/youtube/" + settingsService.Token,
                bodyData);
            return response.Result.IsSuccessStatusCode;
            // var result = await response.Content.ReadAsStringAsync();
        }
    }
}