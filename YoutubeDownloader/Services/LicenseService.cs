using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Language;

namespace YoutubeDownloader.Services
{
    public class LicenseService
    {
        readonly string hwid = libc.hwid.HwId.Generate();

        // need to be lower cased, because of the JSON formatting 
        private record RequestModel(string? hwid = default, int? videoDownloads = default,
            long? videoDownloadLength = default);

        private record ResponseModel(int? youtubeCode = -1, bool success = false);


        public async Task<bool> IsLicenseValid(string? license)
        {
            string result;
            var responseModel = new ResponseModel(-1, true);
            try
            {
                var bodyJson = JsonConvert.SerializeObject(new RequestModel(hwid));
                var bodyData = new StringContent(bodyJson, Encoding.UTF8, "application/json");

                var response = await Http.Client.PostAsync(
                    "https://europe-west1-logbookbackend.cloudfunctions.net/api/youtube/" + license, bodyData);
                result = await response.Content.ReadAsStringAsync();
                
                if (!bool.TryParse(result, out _))
                {
                    responseModel = JsonConvert.DeserializeObject<ResponseModel>(result);
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Verbindung zum Lizenzserver hergestellt werden. Bitte versuche es erneut!",
                    exception);
            }
            
            if (!responseModel!.success)
            {
                throw responseModel.youtubeCode switch
                {
                    0 => new Exception(Resources.TokenVerifyView_Invaild_Ex),
                    1 => new Exception(Resources.TokenVerifyView_Disabled_Ex),
                    2 => new Exception(Resources.TokenVerifyView_Expired_Ex),
                    3 => new Exception(Resources.TokenVerifyView_Amount_Ex),
                    _ => new Exception("Um den Downloader zu verwenden benötigst du einen Lizenzschlüssel!")
                };
            }
            return responseModel.success;
        }

        public void UpdateStats(SettingsService settingsService)
        {
            var bodyJson = JsonConvert.SerializeObject(new RequestModel(hwid,
                settingsService.VideoDownloads, settingsService.VideoDownloadsLength));
            var bodyData = new StringContent(bodyJson, Encoding.UTF8, "application/json");

            using var client = Http.Client;
            client.PatchAsync(
                "https://europe-west1-logbookbackend.cloudfunctions.net/api/youtube/" + settingsService.License,
                bodyData);
        }
    }
}