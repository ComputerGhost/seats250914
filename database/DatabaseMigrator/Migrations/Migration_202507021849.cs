using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202507021849)]
public class Migration_202507021849 : Migration
{
    public override void Down()
    {
        Delete.Index("IX_Configuration_DateSaved_DESC").OnTable("Configuration");

        Delete.Index("IX_SeatLocks_SeatId").OnTable("SeatLocks");

        Delete.Index("IX_SeatStatuses_Status").OnTable("SeatStatuses");

        Delete.Index("IX_Seats_SeatStatusId").OnTable("Seats");
        Delete.Index("UX_Seats_Number").OnTable("Seats");
    }

    public override void Up()
    {
        Create.Index("IX_Configuration_DateSaved_DESC")
            .OnTable("Configuration")
            .OnColumn("DateSaved").Descending();

        Create.Index("IX_SeatLocks_SeatId")
            .OnTable("SeatLocks")
            .OnColumn("SeatId");

        Create.Index("IX_SeatStatuses_Status")
            .OnTable("SeatStatuses")
            .OnColumn("Status");

        Create.Index("IX_Seats_SeatStatusId")
            .OnTable("Seats")
            .OnColumn("SeatStatusId");
        Create.Index("UX_Seats_Number")
            .OnTable("Seats")
            .OnColumn("Number")
            .Unique();
    }
}
