using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202506220025)]
public class Migration_202506220025 : Migration
{
    public override void Down()
    {
        Delete.Column("ScheduledCloseDateTime").FromTable("Configuration");
        Delete.Column("ScheduledCloseTimeZone").FromTable("Configuration");
    }

    public override void Up()
    {
        Alter.Table("Configuration").AddColumn("ScheduledCloseDateTime")
            .AsDateTimeOffset()
            .WithDefault(SystemMethods.CurrentDateTimeOffset);
        Alter.Table("Configuration").AddColumn("ScheduledCloseTimeZone")
            .AsAnsiString(32)
            .WithDefaultValue("UTC");
    }
}
