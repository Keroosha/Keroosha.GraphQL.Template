using FluentMigrator;

namespace Keroosha.GraphQL.Web.Database.Migrations.Roles
{
    [MigrationDate(2020, 10, 22, 12, 48)]
    public class CreateUserRoles : AutoReversingMigration
    {
        public override void Up()
        {
            Create.Table("UserRole")
                .WithColumn("Id").AsInt32().PrimaryKey().Identity()
                .WithColumn("UserId").AsInt32().NotNullable()
                .WithColumn("Role").AsString().NotNullable();

            Create.ForeignKey().FromTable("UserRole").ForeignColumn("UserId").ToTable("Users").PrimaryColumn("Id");
            Create.UniqueConstraint().OnTable("UserRole").Columns("UserId", "Role");
        }
    }
}