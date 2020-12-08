using Microsoft.EntityFrameworkCore.Migrations;

namespace CocApi.Cache.Migrations
{
    public partial class Migration5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WarLogs_Id",
                table: "WarLogs",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Id",
                table: "Groups",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClanWars_Id",
                table: "ClanWars",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clans_Id",
                table: "Clans",
                column: "Id",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WarLogs_Id",
                table: "WarLogs");

            migrationBuilder.DropIndex(
                name: "IX_Groups_Id",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_ClanWars_Id",
                table: "ClanWars");

            migrationBuilder.DropIndex(
                name: "IX_Clans_Id",
                table: "Clans");
        }
    }
}
