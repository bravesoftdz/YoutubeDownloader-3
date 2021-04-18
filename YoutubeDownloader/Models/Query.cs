namespace YoutubeDownloader.Models
{
    public class Query
    {
        public Query(QueryKind kind, string value)
        {
            Kind = kind;
            Value = value;
        }

        public QueryKind Kind { get; }

        public string Value { get; }
    }
}