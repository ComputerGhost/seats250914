using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202507142213)]
public class Migration_202507142213 : Migration
{
    public override void Down()
    {
        Delete.Column("MaxSeatsPerReservation").FromTable("Configuration");
    }

    public override void Up()
    {
        Alter.Table("Configuration")
            .AddColumn("MaxSeatsPerReservation").AsInt32().WithDefaultValue(1);
    }
}
