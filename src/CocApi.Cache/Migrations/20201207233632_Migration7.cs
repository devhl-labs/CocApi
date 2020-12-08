using Microsoft.EntityFrameworkCore.Migrations;

namespace CocApi.Cache.Migrations
{
    public partial class Migration7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Wars_IsFinal",
                table: "Wars",
                column: "IsFinal");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_LocalExpiration",
                table: "Wars",
                column: "LocalExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_ServerExpiration",
                table: "Wars",
                column: "ServerExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_State",
                table: "Wars",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_WarLogs_LocalExpiration",
                table: "WarLogs",
                column: "LocalExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_WarLogs_ServerExpiration",
                table: "WarLogs",
                column: "ServerExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_Players_LocalExpiration",
                table: "Players",
                column: "LocalExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_Players_ServerExpiration",
                table: "Players",
                column: "ServerExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_LocalExpiration",
                table: "Groups",
                column: "LocalExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ServerExpiration",
                table: "Groups",
                column: "ServerExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_ClanWars_LocalExpiration",
                table: "ClanWars",
                column: "LocalExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_ClanWars_ServerExpiration",
                table: "ClanWars",
                column: "ServerExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_Clans_DownloadCurrentWar",
                table: "Clans",
                column: "DownloadCurrentWar");

            migrationBuilder.CreateIndex(
                name: "IX_Clans_LocalExpiration",
                table: "Clans",
                column: "LocalExpiration");

            migrationBuilder.CreateIndex(
                name: "IX_Clans_ServerExpiration",
                table: "Clans",
                column: "ServerExpiration");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wars_IsFinal",
                table: "Wars");

            migrationBuilder.DropIndex(
                name: "IX_Wars_LocalExpiration",
                table: "Wars");

            migrationBuilder.DropIndex(
                name: "IX_Wars_ServerExpiration",
                table: "Wars");

            migrationBuilder.DropIndex(
                name: "IX_Wars_State",
                table: "Wars");

            migrationBuilder.DropIndex(
                name: "IX_WarLogs_LocalExpiration",
                table: "WarLogs");

            migrationBuilder.DropIndex(
                name: "IX_WarLogs_ServerExpiration",
                table: "WarLogs");

            migrationBuilder.DropIndex(
                name: "IX_Players_LocalExpiration",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Players_ServerExpiration",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Groups_LocalExpiration",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_ServerExpiration",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_ClanWars_LocalExpiration",
                table: "ClanWars");

            migrationBuilder.DropIndex(
                name: "IX_ClanWars_ServerExpiration",
                table: "ClanWars");

            migrationBuilder.DropIndex(
                name: "IX_Clans_DownloadCurrentWar",
                table: "Clans");

            migrationBuilder.DropIndex(
                name: "IX_Clans_LocalExpiration",
                table: "Clans");

            migrationBuilder.DropIndex(
                name: "IX_Clans_ServerExpiration",
                table: "Clans");
        }
    }
}
