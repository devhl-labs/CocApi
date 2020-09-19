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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(nullable: false),
                    Downloaded = table.Column<DateTime>(nullable: false),
                    ServerExpiration = table.Column<DateTime>(nullable: false),
                    LocalExpiration = table.Column<DateTime>(nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: false),
                    Download = table.Column<bool>(nullable: false),
                    DownloadMembers = table.Column<bool>(nullable: false),
                    DownloadCurrentWar = table.Column<bool>(nullable: false),
                    DownloadCwl = table.Column<bool>(nullable: false),
                    IsWarLogPublic = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClanWars",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(nullable: false),
                    Downloaded = table.Column<DateTime>(nullable: false),
                    ServerExpiration = table.Column<DateTime>(nullable: false),
                    LocalExpiration = table.Column<DateTime>(nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: false),
                    State = table.Column<int>(nullable: true),
                    PreparationStartTime = table.Column<DateTime>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanWars", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(nullable: false),
                    Downloaded = table.Column<DateTime>(nullable: false),
                    ServerExpiration = table.Column<DateTime>(nullable: false),
                    LocalExpiration = table.Column<DateTime>(nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: false),
                    Season = table.Column<DateTime>(nullable: false),
                    State = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(nullable: false),
                    Downloaded = table.Column<DateTime>(nullable: false),
                    ServerExpiration = table.Column<DateTime>(nullable: false),
                    LocalExpiration = table.Column<DateTime>(nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: false),
                    Download = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WarLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(nullable: false),
                    Downloaded = table.Column<DateTime>(nullable: false),
                    ServerExpiration = table.Column<DateTime>(nullable: false),
                    LocalExpiration = table.Column<DateTime>(nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Wars",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RawContent = table.Column<string>(nullable: false),
                    Downloaded = table.Column<DateTime>(nullable: false),
                    ServerExpiration = table.Column<DateTime>(nullable: false),
                    LocalExpiration = table.Column<DateTime>(nullable: false),
                    StatusCode = table.Column<int>(nullable: false),
                    ClanTag = table.Column<string>(nullable: false),
                    OpponentTag = table.Column<string>(nullable: false),
                    PreparationStartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    WarTag = table.Column<string>(nullable: true),
                    State = table.Column<int>(nullable: true),
                    IsFinal = table.Column<bool>(nullable: false),
                    Season = table.Column<DateTime>(nullable: true),
                    Announcements = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false)
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
                name: "IX_ClanWars_Tag_PreparationStartTime",
                table: "ClanWars",
                columns: new[] { "Tag", "PreparationStartTime" },
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
                name: "IX_WarLogs_Tag",
                table: "WarLogs",
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
                name: "IX_Wars_WarTag",
                table: "Wars",
                column: "WarTag");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_PreparationStartTime_ClanTag",
                table: "Wars",
                columns: new[] { "PreparationStartTime", "ClanTag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wars_PreparationStartTime_OpponentTag",
                table: "Wars",
                columns: new[] { "PreparationStartTime", "OpponentTag" },
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
                name: "WarLogs");

            migrationBuilder.DropTable(
                name: "Wars");
        }
    }
}
