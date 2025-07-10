using FluentMigrator;
using FluentMigrator.SqlServer;

namespace DatabaseMigrator.Migrations;

[Migration(202507090810)]
public class Migration_202507090810 : Migration
{
    public override void Down()
    {
        Alter.Column("Email").OnTable("Reservations")
            .AsString(255);

        Delete.Table("EmailQueue");
        Delete.Table("EmailTypes");
    }

    public override void Up()
    {
        // Adjust email length because SMTP doesn't support over 254 length.
        Alter.Column("Email").OnTable("Reservations")
            .AsString(254);

        // Add tables for email service:

        Create.Table("EmailTypes")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("Name").AsAnsiString(50);

        Insert.IntoTable("EmailTypes").WithIdentityInsert().Row(new { Id = 1, Name = "UserSubmittedReservation" });

        Create.Table("EmailQueue")
            .WithColumn("Id").AsInt32().PrimaryKey().Identity()
            .WithColumn("EmailTypeId").AsInt32()
            .WithColumn("ReferenceId").AsInt32()
            .WithColumn("IsSent").AsBoolean().WithDefaultValue(false)
            .WithColumn("AttemptCount").AsInt32().WithDefaultValue(0)
            .WithColumn("LastAttempt").AsDateTimeOffset().Nullable();

        Create.ForeignKey("FK_EmailQueue_EmailTypes")
            .FromTable("EmailQueue").ForeignColumns("EmailTypeId")
            .ToTable("EmailTypes").PrimaryColumn("Id");
    }
}
