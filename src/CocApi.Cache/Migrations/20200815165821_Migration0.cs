using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CocApi.Cache.Migrations
{
    public partial class Migration0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectState = table.Column<int>(type: "INTEGER", nullable: false),
                    QueryType = table.Column<int>(type: "INTEGER", nullable: true),
                    ClanTag = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadClan = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadVillages = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadCurrentWar = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadCwl = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Path = table.Column<string>(type: "TEXT", nullable: false),
                    Raw = table.Column<string>(type: "TEXT", nullable: false),
                    DownloadDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServerExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocalExpirationDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Villages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VillageTag = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Villages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ObjectState = table.Column<int>(type: "INTEGER", nullable: false),
                    QueryType = table.Column<int>(type: "INTEGER", nullable: true),
                    ClanTag = table.Column<string>(type: "TEXT", nullable: false),
                    OpponentTag = table.Column<string>(type: "TEXT", nullable: false),
                    Json = table.Column<string>(type: "TEXT", nullable: false),
                    PrepStartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    WarTag = table.Column<string>(type: "TEXT", nullable: true),
                    WarState = table.Column<int>(type: "INTEGER", nullable: false),
                    IsFinal = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAvailableByClan = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAvailableByOpponent = table.Column<bool>(type: "INTEGER", nullable: false),
                    Announcements = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wars", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clans_ClanTag",
                table: "Clans",
                column: "ClanTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_Path",
                table: "Items",
                column: "Path",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wars_ClanTag",
                table: "Wars",
                column: "ClanTag");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_OpponentTag",
                table: "Wars",
                column: "OpponentTag");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_PrepStartTime_ClanTag",
                table: "Wars",
                columns: new[] { "PrepStartTime", "ClanTag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wars_PrepStartTime_OpponentTag",
                table: "Wars",
                columns: new[] { "PrepStartTime", "OpponentTag" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clans");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Villages");

            migrationBuilder.DropTable(
                name: "Wars");
        }
    }
}
