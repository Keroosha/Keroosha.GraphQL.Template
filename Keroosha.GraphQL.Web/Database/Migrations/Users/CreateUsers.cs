using FluentMigrator;

namespace Keroosha.GraphQL.Web.Database.Migrations.Users
{
    [MigrationDate(2020, 10, 6, 12, 48)]
    public class CreateUsers : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("Users")
                .WithColumn("Id").AsInt64().PrimaryKey().Identity()
                .WithColumn("Email").AsString().Nullable().Unique()
                .WithColumn("PasswordHash").AsString().Nullable()
                .WithColumn("AvatarImage").AsInt64().Nullable()
                .WithColumn("Confirmed").AsBoolean()
                .WithColumn("ConfirmationCode").AsString().Nullable()
                .WithColumn("Name").AsString();
        }
    }
}