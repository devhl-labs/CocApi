using Microsoft.EntityFrameworkCore.Migrations;

namespace CocApi.Test.Migrations
{
    public partial class Migration11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_current_war_Added_CachedClanId_State",
                table: "current_war",
                columns: new[] { "Added", "CachedClanId", "State" });

            migrationBuilder.CreateIndex(
                name: "IX_current_war_CachedClanId_Download",
                table: "current_war",
                columns: new[] { "CachedClanId", "Download" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_current_war_Added_CachedClanId_State",
                table: "current_war");

            migrationBuilder.DropIndex(
                name: "IX_current_war_CachedClanId_Download",
                table: "current_war");
        }
    }
}
