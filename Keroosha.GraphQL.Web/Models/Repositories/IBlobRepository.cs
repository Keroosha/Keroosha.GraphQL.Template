namespace Keroosha.GraphQL.Web.Models.Repositories
{
    public interface IBlobRepository
    {
        long CreateBlobId();
        void DeleteBlob(long id);
    }
}