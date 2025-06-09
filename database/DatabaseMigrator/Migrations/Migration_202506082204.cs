using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202506082204)]
public class Migration_202506082204 : Migration
{
    public override void Down()
    {
        Delete.Table("Users");
    }

    public override void Up()
    {
        Create.Table("Users")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Login").AsString(8).Unique()
            .WithColumn("PasswordHash").AsAnsiString()
            .WithColumn("IsEnabled").AsBoolean()
        ;
    }
}
