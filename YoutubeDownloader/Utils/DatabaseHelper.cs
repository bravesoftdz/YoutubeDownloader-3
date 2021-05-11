using System.Data;
using System.Threading.Tasks;
using MySqlConnector;
using YoutubeDownloader.Language;
using YoutubeDownloader.Utils.Token;

namespace YoutubeDownloader.Utils
{
    public class DatabaseHelper
    {
        private const string ConnectionString =
            "Server=nuerk-solutions.de;User Id=ytdl;Password=YTDL-2021!;Database=ytdl";

        public DatabaseHelper()
        {
            MySqlConnection = new MySqlConnection(ConnectionString);
        }

        private MySqlConnection MySqlConnection { get; }

        public async Task<MySqlConnection> OpenConnection()
        {
            if (MySqlConnection.State == ConnectionState.Open)
                return MySqlConnection;

            await MySqlConnection.OpenAsync();
            if (MySqlConnection.State == ConnectionState.Open)
                return MySqlConnection;
            throw new TokenException(Resources.TokenVerifyView_NoConnection_Ex);
        }

        public async Task CloseConnection()
        {
            await MySqlConnection.CloseAsync();
        }
    }
}