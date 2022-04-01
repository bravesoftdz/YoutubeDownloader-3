using System;
using System.Diagnostics;
using System.IO;
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
        // need to be lower cased, because of the JSON formatting 
        private record RequestModel(string? hwid = default, int? videoDownloads = default,
            long? videoDownloadLength = default);

        private record ResponseModel(int? youtubeCode = -1, bool success = false);


        public async Task<bool> IsTokenValid(string? token)
        {
            var result = "";
            try
            {
                var bodyJson = JsonConvert.SerializeObject(new RequestModel(HWIDGenerator.UID));
                var bodyData = new StringContent(bodyJson, Encoding.UTF8, "application/json");

                var response = await Http.Client.PostAsync(
                    "https://europe-west1-logbookbackend.cloudfunctions.net/api/youtube/" + token, bodyData);
                result = await response.Content.ReadAsStringAsync();
            }
            catch (Exception exception)
            {
                throw new TokenException("Verbindung zum Lizenzserver hergestellt werden. Bitte versuche es erneut!",
                    exception);
            }

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
                    _ => new TokenException("Um den Downloader zu verwenden benötigst du einen Lizenzschlüssel!")
                };
            }

            return responseModel.success;
        }

        public void UpdateStats(SettingsService settingsService)
        {
            var bodyJson = JsonConvert.SerializeObject(new RequestModel(HWIDGenerator.UID,
                settingsService.VideoDownloads, settingsService.VideoDownloadsLength));
            var bodyData = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            using var client = Http.Client;
            client.PatchAsync(
                "https://europe-west1-logbookbackend.cloudfunctions.net/api/youtube/" + settingsService.Token,
                bodyData);
            // using var reader = new StreamReader(response.Content.ReadAsStream());
            // var result = await response.Content.ReadAsStringAsync();
        }
    }
}