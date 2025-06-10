using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202506101752)]
public class Migration_202506101752 : Migration
{
    public override void Down()
    {
        Delete.Column("DateSaved").FromTable("Configuration");
    }

    public override void Up()
    {
        Alter.Table("Configuration").AddColumn("DateSaved")
            .AsDateTimeOffset()
            .WithDefault(SystemMethods.CurrentDateTimeOffset);
    }
}
