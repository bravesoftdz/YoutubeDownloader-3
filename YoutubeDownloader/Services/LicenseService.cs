using System;
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
        public bool isVerified { get; private set; }

        // need to be lower cased, because of the JSON formatting 
        private record RequestModel(string? hwid = default, int? videoDownloads = default,
            long? videoDownloadLength = default);

        private record ResponseModel(int? youtubeCode = -1, bool success = false);


        public async Task<bool> IsLicenseValid(string? license)
        {
            var responseModel = new ResponseModel(-1, true);
            try
            {
                var bodyJson = JsonConvert.SerializeObject(new RequestModel(hwid));
                var bodyData = new StringContent(bodyJson, Encoding.UTF8, "application/json");

                var response = await Http.Client.PostAsync(
                    "https://europe-west1-logbookbackend.cloudfunctions.net/api/youtube/" + license, bodyData);
                var result = await response.Content.ReadAsStringAsync();
                
                if (!bool.TryParse(result, out _))
                {
                    responseModel = JsonConvert.DeserializeObject<ResponseModel>(result);
                }
            }
            catch (Exception exception)
            {
                throw new Exception(Resources.LicenseService_InvalidConnection,
                    exception);
            }
            
            if (!responseModel!.success)
            {
                isVerified = false;
                throw responseModel.youtubeCode switch
                {
                    0 => new Exception(Resources.LicenseService_invalid),
                    1 => new Exception(Resources.LicenseService_disabled),
                    2 => new Exception(Resources.LicenseService_expired),
                    3 => new Exception(Resources.LicenseService_amount),
                    _ => new Exception(Resources.LicenseService_need_a_license)
                };
            }
            return isVerified = true;
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