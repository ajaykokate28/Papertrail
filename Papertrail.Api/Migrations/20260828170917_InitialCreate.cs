using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Papertrail.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GenerationHistory",
                columns: table => new
                {
                    RequestId = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    ConversationId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Prompt = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Answer = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GenerationHistory", x => x.RequestId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GenerationHistory_ConversationId_CreatedAt",
                table: "GenerationHistory",
                columns: new[] { "ConversationId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GenerationHistory");
        }
    }
}
