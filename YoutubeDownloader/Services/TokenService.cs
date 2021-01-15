using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Tyrrrz.Extensions;
using YoutubeDownloader.Internal;
using YoutubeDownloader.Internal.HWID;
using YoutubeDownloader.Internal.Token;
using System.Linq;

namespace YoutubeDownloader.Services
{
    public class TokenService
    {
        private readonly HttpClient _httpClient = new();
        private List<TokenEx> _tokens = new List<TokenEx>();
        private string connectionString = "Server=95.156.227.125;User ID=ytclient;Password=YTDL-2021;Database=YTDL_TOKENS";


        public TokenService()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"{App.Name} ({App.GitHubProjectUrl})");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "YoutubeDownloader (github.com/derech1e/YoutubeDownloader)");
            _httpClient.DefaultRequestHeaders.Add("Authorization", "token 448d39603553439c25adb24e11ed666bb5724e17");
        }

        public async Task CacheJsonMariaDB()
        {
            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            if (connection.State != System.Data.ConnectionState.Open)
                return;

            using var command = new MySqlCommand("SELECT * FROM Tokens;", connection);
            using var reader = command.ExecuteReader();
            while (await reader.ReadAsync())
                _tokens.Add(new TokenEx(reader.GetInt16(0), reader.GetInt16(2) != 0, reader.GetInt16(3), reader.GetString(4), await reader.IsDBNullAsync(5) ? DateTime.MaxValue : reader.GetDateTime(5), await reader.IsDBNullAsync(6) ? string.Empty : reader.GetString(6)));

            await connection.CloseAsync();
        }

        public async Task<bool?> IsTokenVaild(string tokenFromInput, SettingsService settingsService)
        {
            try
            {
                await CacheJsonMariaDB();
            }
            catch (MySqlException)
            {
                return !settingsService.Token.IsNullOrEmpty();
            }

            TokenEx token = _tokens.Where(token => token.Token!.Equals(tokenFromInput.Trim())).FirstOrDefault();

            if (token == null)
                throw new TokenException(Language.Resources.TokenVerifyView_Invaild_Ex);
            if (!(bool)token.Enabled!)
                throw new TokenException(Language.Resources.TokenVerifyView_Disabled_Ex);
            if (token.Amount == 0)
                if (token.HWID != HWIDGenerator.UID)
                    throw new TokenException(Language.Resources.TokenVerifyView_Amount_Ex);
            if (token.ExpiryDate < DateTime.Now)
                throw new TokenException(Language.Resources.TokenVerifyView_Expired_Ex);

            if (token.Amount != 0)
            {
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                if (connection.State != System.Data.ConnectionState.Open)
                    throw new TokenException(Language.Resources.TokenVerifyView_NoConnection_Ex);

                // Insert some data
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.CommandText = "UPDATE `YTDL_TOKENS`.`Tokens` SET `Amount`=@amount, `HWID`=@hwid WHERE  `id`=@id;";
                    cmd.Parameters.AddWithValue("amount", (token.Amount - 1));
                    cmd.Parameters.AddWithValue("hwid", HWIDGenerator.UID);
                    cmd.Parameters.AddWithValue("id", token.ID);
                    await cmd.ExecuteNonQueryAsync();

                }

                await connection.CloseAsync();
            }
            return true;
        }
    }
}
