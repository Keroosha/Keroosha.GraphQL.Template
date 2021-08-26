namespace Keroosha.GraphQL.Web.Config
{
    public enum BlobTypes
    {
        Local
    }

    public class LocalBlobConfig
    {
        public string Path { get; set; }
    }

    public class BlobConfig
    {
        public BlobTypes Type { get; set; } = BlobTypes.Local;
        public LocalBlobConfig Local { get; set; } = new();
    }
}