using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CocApi.Cache.Migrations
{
    public partial class Migration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WarLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(type: "TEXT", nullable: true),
                    Downloaded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServerExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocalExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarLogs_Tag",
                table: "WarLogs",
                column: "Tag",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarLogs");
        }
    }
}
