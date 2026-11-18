using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SupportTickets.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    title = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    priority = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.id);
                    table.CheckConstraint("CK_Tickets_Priority", "[priority] IN ('low', 'medium', 'high')");
                    table.CheckConstraint("CK_Tickets_Status", "[status] IN ('open', 'in_progress', 'closed')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_priority",
                table: "Tickets",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_status",
                table: "Tickets",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
