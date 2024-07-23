using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CocApi.Test.Migrations
{
    /// <inheritdoc />
    public partial class Migration15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_war_log_ExpiresAt",
                table: "war_log");

            migrationBuilder.DropIndex(
                name: "IX_war_log_KeepUntil",
                table: "war_log");

            migrationBuilder.DropIndex(
                name: "IX_war_ExpiresAt",
                table: "war");

            migrationBuilder.DropIndex(
                name: "IX_war_KeepUntil",
                table: "war");

            migrationBuilder.DropIndex(
                name: "IX_war_Season",
                table: "war");

            migrationBuilder.DropIndex(
                name: "IX_war_WarTag",
                table: "war");

            migrationBuilder.DropIndex(
                name: "IX_player_ExpiresAt",
                table: "player");

            migrationBuilder.DropIndex(
                name: "IX_player_KeepUntil",
                table: "player");

            migrationBuilder.DropIndex(
                name: "IX_group_ExpiresAt",
                table: "group");

            migrationBuilder.DropIndex(
                name: "IX_group_KeepUntil",
                table: "group");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_war_log_ExpiresAt",
                table: "war_log",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_war_log_KeepUntil",
                table: "war_log",
                column: "KeepUntil");

            migrationBuilder.CreateIndex(
                name: "IX_war_ExpiresAt",
                table: "war",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_war_KeepUntil",
                table: "war",
                column: "KeepUntil");

            migrationBuilder.CreateIndex(
                name: "IX_war_Season",
                table: "war",
                column: "Season");

            migrationBuilder.CreateIndex(
                name: "IX_war_WarTag",
                table: "war",
                column: "WarTag");

            migrationBuilder.CreateIndex(
                name: "IX_player_ExpiresAt",
                table: "player",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_player_KeepUntil",
                table: "player",
                column: "KeepUntil");

            migrationBuilder.CreateIndex(
                name: "IX_group_ExpiresAt",
                table: "group",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_group_KeepUntil",
                table: "group",
                column: "KeepUntil");
        }
    }
}
