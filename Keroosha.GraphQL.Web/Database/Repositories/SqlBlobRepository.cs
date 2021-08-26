using System.Linq;
using Keroosha.GraphQL.Web.Models;
using Keroosha.GraphQL.Web.Models.Repositories;
using LinqToDB;

namespace Keroosha.GraphQL.Web.Database.Repositories
{
    public class SqlBlobRepository : IBlobRepository
    {
        private readonly AppDbContextManager _db;

        public SqlBlobRepository(AppDbContextManager db)
        {
            _db = db;
        }

        public long CreateBlobId()
        {
            return _db.Exec(db => db.InsertWithInt64Identity(new Blob()));
        }

        public void DeleteBlob(long id) =>
            _db.Exec(db => db.Blobs.Select(x => x.Id == id).Delete());
    }
}