using FluentMigrator;
using System.Data;

namespace DatabaseMigrator.Migrations;

[Migration(202507190021)]
public class Migration_202507190021 : Migration
{
    public override void Down()
    {
        // Originally added in 202506110128
        Create.Column("SeatId").OnTable("Reservations")
            .AsInt32();
        Create.Column("SeatLockId").OnTable("Reservations")
            .AsInt32().Nullable();
        Create.ForeignKey("FK_Reservations_Seats")
            .FromTable("Reservations").ForeignColumn("SeatId")
            .ToTable("Seats").PrimaryColumn("Id");
        Create.ForeignKey("FK_Reservations_SeatLocks")
            .FromTable("Reservations").ForeignColumn("SeatLockId")
            .ToTable("SeatLocks").PrimaryColumn("Id")
            .OnDelete(Rule.SetNull);

        // Originally added in 202507060741
        Execute.Sql("""
            CREATE UNIQUE INDEX UX_Reservations_SeatLockId_NotNull
            ON Reservations(SeatLockId)
            WHERE SeatLockId IS NOT NULL;
            """);
    }

    public override void Up()
    {
        // Originally added in 202507060741
        Delete.Index("UX_Reservations_SeatLockId_NotNull").OnTable("Reservations");

        // Originally added in 202506110128
        Delete.ForeignKey("FK_Reservations_Seats").OnTable("Reservations");
        Delete.ForeignKey("FK_Reservations_SeatLocks").OnTable("Reservations");
        Delete.Column("SeatId").FromTable("Reservations");
        Delete.Column("SeatLockId").FromTable("Reservations");
    }
}
