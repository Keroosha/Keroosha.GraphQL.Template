using FluentMigrator;

namespace Keroosha.GraphQL.Web.Database.Migrations.Blobs
{
    [MigrationDate(2020, 10, 8, 16, 05)]
    public class MigrateCreateBlobs : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Blobs")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity();
        }
    }
}