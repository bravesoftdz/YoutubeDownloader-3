using System;

namespace YoutubeDownloader.Utils.Token
{
    public class TokenEx
    {
        public TokenEx(int? id, bool? enabled, string? token, DateTime? expiryDate, string? hwid, bool? systemBind)
        {
            Id = id;
            Enabled = enabled;
            Token = token;
            ExpiryDate = expiryDate;
            Hwid = hwid;
            SystemBind = systemBind;
        }

        public int? Id { get; }
        public bool? Enabled { get; }
        public string? Token { get; }
        public DateTime? ExpiryDate { get; }
        public string? Hwid { get; }
        public bool? SystemBind { get; }
    }
}