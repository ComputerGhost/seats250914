using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202507160516)]
public class Migration_202507160516 : Migration
{
    public override void Down()
    {
        Delete.Table("ReservationSeatLocks");
    }

    public override void Up()
    {
        Create.Table("ReservationSeatLocks")
            .WithColumn("ReservationId").AsInt32()
            .WithColumn("SeatLockId").AsInt32();

        Create.ForeignKey("FK_ReservationSeatLocks_Reservations")
            .FromTable("ReservationSeatLocks").ForeignColumn("ReservationId")
            .ToTable("Reservations").PrimaryColumn("Id");
        Create.ForeignKey("FK_ReservationSeatLocks_SeatLocks")
            .FromTable("ReservationSeatLocks").ForeignColumn("SeatLockId")
            .ToTable("SeatLocks").PrimaryColumn("Id");

        Create.UniqueConstraint("UQ_ReservationSeatLocks")
            .OnTable("ReservationSeatLocks")
            .Columns("ReservationId", "SeatLockId");
        Create.UniqueConstraint("UQ_ReservationSeatLocks_SeatLockId")
            .OnTable("ReservationSeatLocks")
            .Column("SeatLockId");
    }
}
