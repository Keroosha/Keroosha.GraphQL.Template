using LinqToDB.Mapping;

namespace Keroosha.GraphQL.Web.Models
{
    [Table("Blobs")]
    public class Blob
    {
        [PrimaryKey, Identity] public long Id { get; set; }
    }
}