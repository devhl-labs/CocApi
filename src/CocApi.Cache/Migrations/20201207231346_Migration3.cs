using Microsoft.EntityFrameworkCore.Migrations;

namespace CocApi.Cache.Migrations
{
    public partial class Migration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Players_Id",
                table: "Players",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clans_IsWarLogPublic",
                table: "Clans",
                column: "IsWarLogPublic");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_Id",
                table: "Players");

            migrationBuilder.DropIndex(
                name: "IX_Clans_IsWarLogPublic",
                table: "Clans");
        }
    }
}
