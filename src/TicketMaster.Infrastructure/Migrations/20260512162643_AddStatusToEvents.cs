using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketMaster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: All other tables already exist in the database
            // (created before EF Migrations were introduced).
            // This migration only applies the net-new change.
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Events");
        }
    }
}
