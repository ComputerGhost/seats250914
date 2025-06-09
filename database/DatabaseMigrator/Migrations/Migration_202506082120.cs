using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202506082120)]
public class Migration_202506082120 : Migration
{
    public override void Down()
    {
        Delete.Table("Configuration");
    }

    public override void Up()
    {
        Create.Table("Configuration")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("ForceCloseReservations").AsBoolean()
            .WithColumn("ForceOpenReservations").AsBoolean()
            .WithColumn("GracePeriodSeconds").AsInt32()
            .WithColumn("MaxSeatsPerPerson").AsInt32()
            .WithColumn("MaxSeatsPerIPAddress").AsInt32()
            .WithColumn("MaxSecondsToConfirmSeat").AsInt32()
            .WithColumn("ScheduledOpenDateTime").AsDateTimeOffset()
            .WithColumn("ScheduledOpenTimeZone").AsAnsiString(32)
        ;
    }
}
