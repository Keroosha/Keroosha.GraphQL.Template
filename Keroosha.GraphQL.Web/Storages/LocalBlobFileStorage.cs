using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Keroosha.GraphQL.Web.Storages
{
    public class LocalBlobFileStorage : IBlobFileStorage
    {
        private readonly string _blobsPath;

        public LocalBlobFileStorage(string blobsPath)
        {
            _blobsPath = blobsPath;
        }

        public async Task CreateBlobAsync(long id, Stream s)
        {
            var path = GetPath(id, true);
            await using var f = File.Create(path);
            await s.CopyToAsync(f);
        }

        public Stream OpenBlob(long id)
        {
            return File.OpenRead(GetPath(id, false));
        }

        public async Task DeleteBlob(long id)
        {
            var path = GetPath(id, false);
            if (!File.Exists(path)) return;
            File.Delete(path);
            await Task.Yield();
        }

        private string GetPath(long id, bool create)
        {
            var s = id.ToString();
            if (s.Length == 0)
                return Path.Combine(_blobsPath, s + ".bin");

            var ds = s.Substring(0, s.Length - 1);
            var dir = Path.Combine(ds.Select(x => x.ToString()).Prepend(_blobsPath).ToArray());
            if (create)
                Directory.CreateDirectory(dir);
            return Path.Combine(dir, s.Last() + ".bin");
        }
    }
}