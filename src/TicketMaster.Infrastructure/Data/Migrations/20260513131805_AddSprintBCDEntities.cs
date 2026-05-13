using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TicketMaster.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSprintBCDEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CamaroteGroupId",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SvgId",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagemUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CamaroteGroups",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CamaroteGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CamaroteGroups_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventSectorPrices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sector = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MaxQuota = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSectorPrices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSectorPrices_Events_EventId",
                        column: x => x.EventId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PrecosHistoricos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TipoIngressoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrecoAnterior = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecoNovo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AlteradoEm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrecosHistoricos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CamaroteGroupId",
                table: "Tickets",
                column: "CamaroteGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_CamaroteGroups_EventId",
                table: "CamaroteGroups",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSectorPrices_EventId",
                table: "EventSectorPrices",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_CamaroteGroups_CamaroteGroupId",
                table: "Tickets",
                column: "CamaroteGroupId",
                principalTable: "CamaroteGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_CamaroteGroups_CamaroteGroupId",
                table: "Tickets");

            migrationBuilder.DropTable(
                name: "CamaroteGroups");

            migrationBuilder.DropTable(
                name: "EventSectorPrices");

            migrationBuilder.DropTable(
                name: "PrecosHistoricos");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CamaroteGroupId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "CamaroteGroupId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "SvgId",
                table: "Tickets");

            migrationBuilder.DropColumn(
                name: "ImagemUrl",
                table: "Events");
        }
    }
}
