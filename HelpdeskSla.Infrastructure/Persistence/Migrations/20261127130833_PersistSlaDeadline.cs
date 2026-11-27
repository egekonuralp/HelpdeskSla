using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpdeskSla.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PersistSlaDeadline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SlaDeadline",
                table: "Tickets",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.Sql(@"
                UPDATE Tickets
                SET SlaDeadline = DATEADD(HOUR, 
                    CASE Priority
                        WHEN 0 THEN 24  -- Low
                        WHEN 1 THEN 12  -- Medium
                        WHEN 2 THEN 8   -- High
                        WHEN 3 THEN 2   -- Critical
                    END,
                    CreatedAt);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SlaDeadline",
                table: "Tickets");
        }
    }
}
