using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202507160516)]
public class Migration_202507160516 : Migration
{
    public override void Down()
    {
        Delete.Column("ReservationId")
            .FromTable("SeatLocks");
    }

    public override void Up()
    {
        Create.Column("ReservationId")
            .OnTable("SeatLocks")
            .AsInt32().Nullable();

        Create.ForeignKey("FK_SeatLocks_ReservationId")
            .FromTable("SeatLocks").ForeignColumn("ReservationId")
            .ToTable("Reservations").PrimaryColumn("Id");

        Execute.Sql("""
            UPDATE SeatLocks
            SET SeatLocks.ReservationId = Reservations.Id
            FROM Reservations
            WHERE Reservations.SeatLockId = SeatLocks.Id
            """);
    }
}
