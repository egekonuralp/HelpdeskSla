using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpdeskSla.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Tickets",
                type: "rowversion",
                rowVersion: true,
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Tickets");
        }
    }
}
