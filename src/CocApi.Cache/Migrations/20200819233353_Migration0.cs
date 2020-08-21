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
                    RawContent = table.Column<string>(type: "TEXT", nullable: true),
                    Downloaded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServerExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocalExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: false),
                    Download = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadMembers = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadCurrentWar = table.Column<bool>(type: "INTEGER", nullable: false),
                    DownloadCwl = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClanWars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(type: "TEXT", nullable: true),
                    Downloaded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServerExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocalExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: false),
                    State = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanWars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(type: "TEXT", nullable: true),
                    Downloaded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServerExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocalExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: false),
                    Season = table.Column<DateTime>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(type: "TEXT", nullable: true),
                    Downloaded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServerExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocalExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: false),
                    Download = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(type: "TEXT", nullable: true),
                    Downloaded = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ServerExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LocalExpiration = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ClanTag = table.Column<string>(type: "TEXT", nullable: false),
                    OpponentTag = table.Column<string>(type: "TEXT", nullable: false),
                    PrepStartTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    EndTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    WarTag = table.Column<string>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: true),
                    IsFinal = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsAvailableByClan = table.Column<bool>(type: "INTEGER", nullable: true),
                    IsAvailableByOpponent = table.Column<bool>(type: "INTEGER", nullable: true),
                    Announcements = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wars", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clans_Tag",
                table: "Clans",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClanWars_Tag",
                table: "ClanWars",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_Tag",
                table: "Groups",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Tag",
                table: "Players",
                column: "Tag",
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
                name: "ClanWars");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Wars");
        }
    }
}
