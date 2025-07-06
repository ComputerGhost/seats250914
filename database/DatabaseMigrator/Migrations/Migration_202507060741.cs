using FluentMigrator;

namespace DatabaseMigrator.Migrations;

[Migration(202507060741)]
public class Migration_202507060741 : Migration
{
    public override void Down()
    {
        Delete.Index("UX_Reservations_SeatLockId_NotNull")
            .OnTable("Reservations");
    }

    public override void Up()
    {
        /*
         * I want to create a unique constraint on `SeatLockId`,
         * but I don't want it to care about multiple `NULL` values.
         * 
         * The ANSI SQL standard says `NULL != NULL`, so this should work:
         * 
         * ```
         * Create.Index("UX_Reservations_SeatLockId")
         *      .OnTable("Reservations")
         *      .OnColumn("SeatLockId")
         *      .Unique();
         * ```
         * 
         * But not all databases follow the standard.
         * I am using a database-specific query to avoid the edge case.
         */
        Execute.Sql("""
            CREATE UNIQUE INDEX UX_Reservations_SeatLockId_NotNull
            ON Reservations(SeatLockId)
            WHERE SeatLockId IS NOT NULL;
            """);
    }
}
