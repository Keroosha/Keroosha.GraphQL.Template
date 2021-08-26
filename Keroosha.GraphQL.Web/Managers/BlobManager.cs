using System.IO;
using System.Threading.Tasks;
using Keroosha.GraphQL.Web.Models.Repositories;
using Keroosha.GraphQL.Web.Storages;

namespace Keroosha.GraphQL.Web.Managers
{
    public class BlobManager
    {
        private readonly IBlobRepository _repo;
        private readonly IBlobFileStorage _storage;

        public BlobManager(IBlobFileStorage storage, IBlobRepository repo)
        {
            _storage = storage;
            _repo = repo;
        }

        public async Task<long> CreateBlob(Stream s)
        {
            var id = _repo.CreateBlobId();
            await _storage.CreateBlobAsync(id, s);
            return id;
        }

        public async Task<long> CreateBlob(string path)
        {
            await using var f = File.OpenRead(path);
            return await CreateBlob(f);
        }

        public Stream OpenBlob(long blobId)
        {
            return _storage.OpenBlob(blobId);
        }

        public async Task RemoveBlob(long id)
        {
            _repo.DeleteBlob(id);
            await _storage.DeleteBlob(id);
        }
    }
}