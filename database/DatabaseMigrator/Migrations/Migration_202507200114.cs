using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202507200114)]
public class Migration_202507200114 : Migration
{
    public override void Down()
    {
        Delete.Table("ReservationSeats");
    }

    public override void Up()
    {
        Create.Table("ReservationSeats")
            .WithColumn("ReservationId").AsInt32().NotNullable()
            .WithColumn("SeatId").AsInt32().NotNullable();

        Create.PrimaryKey("PK_ReservationSeats")
            .OnTable("ReservationSeats")
            .Columns("ReservationId", "SeatId");

        Create.ForeignKey("FK_ReservationSeats_Reservations")
            .FromTable("ReservationSeats").ForeignColumn("ReservationId")
            .ToTable("Reservations").PrimaryColumn("Id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.ForeignKey("FK_ReservationSeats_Seats")
            .FromTable("ReservationSeats").ForeignColumn("SeatId")
            .ToTable("Seats").PrimaryColumn("Id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);
    }
}
