using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using MySqlConnector;
using Tyrrrz.Extensions;
using YoutubeDownloader.Language;
using YoutubeDownloader.Utils.Token;
using YoutubeDownloader.Utils.Token.HWID;

namespace YoutubeDownloader.Services
{
    public class TokenService
    {
        private const string ConnectionString =
            "Server=nuerk-solutions.de;User Id=ytdl;Password=YTDL-2021!;Database=ytdl";

        private readonly HttpClient _httpClient = new();
        private readonly List<TokenEx> _tokens = new();

        public TokenService()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{App.Name} ({App.GitHubProjectUrl})");
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "YoutubeDownloader (github.com/derech1e/YoutubeDownloader)");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "token 448d39603553439c25adb24e11ed666bb5724e17");
        }

        public bool IsReady { get; private set; }

        private async Task CacheJsonMariaDb()
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            if (connection.State != ConnectionState.Open)
                return;

            await using var command = new MySqlCommand("SELECT * FROM Tokens;", connection);
            using var reader = command.ExecuteReaderAsync();
            while (await reader.Result.ReadAsync())
                _tokens.Add(new TokenEx(reader.Result.GetInt32(0), reader.Result.GetByte(2) != 0,
                    reader.Result.GetString(3),
                    await reader.Result.IsDBNullAsync(4) ? DateTime.MaxValue : reader.Result.GetDateTime(4),
                    await reader.Result.IsDBNullAsync(5) ? "NULL" : reader.Result.GetString(5),
                    reader.Result.GetByte(6) != 0));

            await connection.CloseAsync();
        }

        public async Task<bool?> IsTokenValid(string? tokenFromInput, SettingsService settingsService, bool startUp)
        {
            try
            {
                await CacheJsonMariaDb();
            }
            catch (MySqlException exception)
            {
                var exitBox = MessageBox.Show(
                    Resources.TokenVerifyView_NoConnection_Ex + "\n" + exception.StackTrace,
                    Resources.MessageBoxView_Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop);
                if (exitBox == MessageBoxResult.OK)
                    Application.Current.Shutdown();
                return !settingsService.Token.IsNullOrEmpty();
            }
            var token = _tokens.SingleOrDefault(tokenEx => tokenEx.Token!.Equals(tokenFromInput!.Trim()));

            if (!await MatchTokenRequirements(token!)) return false;

            if (startUp) //Startup check, to avoid unnecessary subtraction
            {
                IsReady = true;
                return true;
            }

            IsReady = true;
            return true;
        }


        private static async Task UpdateDatabase(TokenEx tokenEx)
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            if (connection.State != ConnectionState.Open)
                throw new TokenException(Resources.TokenVerifyView_NoConnection_Ex);

            await using (var cmd = new MySqlCommand())
            {
                cmd.Connection = connection;
                cmd.CommandText =
                    "UPDATE `ytdl`.`Tokens` SET `Hwid`=@hwid WHERE `id`=@id;";
                cmd.Parameters.AddWithValue("hwid", HWIDGenerator.UID);
                cmd.Parameters.AddWithValue("id", tokenEx.Id);
                await cmd.ExecuteNonQueryAsync();
            }

            await connection.CloseAsync();
        }

        private static async Task<bool> MatchTokenRequirements(TokenEx tokenEx)
        {
            if (tokenEx.Token!.IsNullOrEmpty())
                throw new TokenException(Resources.TokenVerifyView_Invaild_Ex);

            if (!(bool) tokenEx.Enabled!)
                throw new TokenException(Resources.TokenVerifyView_Disabled_Ex);

            if (tokenEx.ExpiryDate! < DateTime.Now)
                throw new TokenException(Resources.TokenVerifyView_Expired_Ex);

            if (!(bool) tokenEx.SystemBind!) return true;
            if (tokenEx.Hwid! == HWIDGenerator.UID) return true;
            if (!tokenEx.Hwid!.Equals("NULL"))
                throw new TokenException(Resources.TokenVerifyView_Amount_Ex);
            await UpdateDatabase(tokenEx);
            return true;
        }
    }
}