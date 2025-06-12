using FluentMigrator;
using System.Data;

namespace DatabaseMigrator.Migrations;

[Migration(202506110128)]
public class Migration_202506110128 : Migration
{
    public override void Down()
    {
        Delete.Table("Reservations");
        Delete.Table("SeatLocks");
        Delete.Table("Seats");
    }

    public override void Up()
    {
        AddSeatsTable();
        AddSeatLocksTable();
        AddReservationsTable();
    }

    private void AddSeatsTable()
    {
        const int SEAT_COUNT = 100;

        Create.Table("SeatStatuses")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Status").AsAnsiString(50);

        Create.UniqueConstraint("UQ_SeatStatuses_Status")
            .OnTable("SeatStatuses")
            .Column("Status");

        Insert.IntoTable("SeatStatuses").Row(new { Id = 1, Status = "Available" });
        Insert.IntoTable("SeatStatuses").Row(new { Id = 2, Status = "Locked" });
        Insert.IntoTable("SeatStatuses").Row(new { Id = 3, Status = "AwaitingPayment" });
        Insert.IntoTable("SeatStatuses").Row(new { Id = 4, Status = "ReservationConfirmed" });

        Create.Table("Seats")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SeatStatusId").AsInt32()
            .WithColumn("Number").AsInt32();

        Create.ForeignKey("FK_Seats_SeatStatuses")
            .FromTable("Seats").ForeignColumn("SeatStatusId")
            .ToTable("SeatStatuses").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);

        for (int number = 1; number <= SEAT_COUNT; ++number)
        {
            Insert.IntoTable("Seats").Row(new
            {
                SeatStatusId = 1, // "Available"
                Number = $"{number}",
            });
        }
    }

    private void AddSeatLocksTable()
    {
        Create.Table("SeatLocks")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SeatId").AsInt32()
            .WithColumn("Key").AsAnsiString(44)
            .WithColumn("LockedAt").AsDateTimeOffset().WithDefault(SystemMethods.CurrentDateTimeOffset)
            .WithColumn("LockExpiresAt").AsDateTimeOffset().Nullable();

        Create.UniqueConstraint("UQ_SeatLocks_SeatId")
            .OnTable("SeatLocks")
            .Column("SeatId");

        Create.ForeignKey("FK_SeatLocks_Seats")
            .FromTable("SeatLocks").ForeignColumn("SeatId")
            .ToTable("Seats").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
    }

    private void AddReservationsTable()
    {
        Create.Table("Reservations")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("SeatLockId").AsInt32()
            .WithColumn("ReservedAt").AsDateTimeOffset().WithDefault(SystemMethods.CurrentDateTimeOffset)
            .WithColumn("Name").AsString(50)
            .WithColumn("Email").AsString(255)
            .WithColumn("PhoneNumber").AsString(15).Nullable()
            .WithColumn("PreferredLanguage").AsString(50);

        Create.ForeignKey("FK_Reservations_SeatLocks")
            .FromTable("Reservations").ForeignColumn("SeatLockId")
            .ToTable("SeatLocks").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);

        Create.ForeignKey("FK_Reservations_Seats")
            .FromTable("Reservations").ForeignColumn("SeatId")
            .ToTable("Seats").PrimaryColumn("Id")
            .OnDeleteOrUpdate(Rule.Cascade);
    }
}
