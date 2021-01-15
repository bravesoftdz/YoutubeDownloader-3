using System;

namespace YoutubeDownloader.Internal
{
    public class TokenEx
    {
        public TokenEx(int? id, bool? enabled, int? amount, string? token, DateTime expiryDate, string? hwid)
        {
            ID = id;
            Enabled = enabled;
            Amount = amount;
            Token = token;
            ExpiryDate = expiryDate;
            HWID = hwid;
        }

        public int? ID { get; } = default;
        public bool? Enabled { get; set; } = default;
        public int? Amount { get; set; } = default;
        public string? Token { get; set; } = default;
        public DateTime ExpiryDate  { get; set; } = default;
        public string? HWID { get; set; } = default;

    }
}
