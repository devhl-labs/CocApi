using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace CocApi.Test.Migrations
{
    public partial class Migration0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clan",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DownloadMembers = table.Column<bool>(type: "boolean", nullable: false),
                    IsWarLogPublic = table.Column<bool>(type: "boolean", nullable: true),
                    Tag = table.Column<string>(type: "text", nullable: false),
                    RawContent = table.Column<string>(type: "text", nullable: true),
                    DownloadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    KeepUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    Download = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clan", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "player",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Tag = table.Column<string>(type: "text", nullable: false),
                    ClanTag = table.Column<string>(type: "text", nullable: true),
                    RawContent = table.Column<string>(type: "text", nullable: true),
                    DownloadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    KeepUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    Download = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_player", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "war",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClanTag = table.Column<string>(type: "text", nullable: false),
                    OpponentTag = table.Column<string>(type: "text", nullable: false),
                    PreparationStartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    WarTag = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: true),
                    IsFinal = table.Column<bool>(type: "boolean", nullable: false),
                    Season = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Announcements = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RawContent = table.Column<string>(type: "text", nullable: true),
                    DownloadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    KeepUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    Download = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_war", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "current_war",
                columns: table => new
                {
                    CachedClanId = table.Column<int>(type: "integer", nullable: false),
                    Added = table.Column<bool>(type: "boolean", nullable: false),
                    EnemyTag = table.Column<string>(type: "text", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: true),
                    PreparationStartTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    RawContent = table.Column<string>(type: "text", nullable: true),
                    DownloadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    KeepUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    Download = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_current_war", x => x.CachedClanId);
                    table.ForeignKey(
                        name: "FK_current_war_clan_CachedClanId",
                        column: x => x.CachedClanId,
                        principalTable: "clan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group",
                columns: table => new
                {
                    CachedClanId = table.Column<int>(type: "integer", nullable: false),
                    Season = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    State = table.Column<int>(type: "integer", nullable: true),
                    Added = table.Column<bool>(type: "boolean", nullable: false),
                    RawContent = table.Column<string>(type: "text", nullable: true),
                    DownloadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    KeepUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    Download = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group", x => x.CachedClanId);
                    table.ForeignKey(
                        name: "FK_group_clan_CachedClanId",
                        column: x => x.CachedClanId,
                        principalTable: "clan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "war_log",
                columns: table => new
                {
                    CachedClanId = table.Column<int>(type: "integer", nullable: false),
                    RawContent = table.Column<string>(type: "text", nullable: true),
                    DownloadedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    KeepUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    StatusCode = table.Column<int>(type: "integer", nullable: true),
                    Download = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_war_log", x => x.CachedClanId);
                    table.ForeignKey(
                        name: "FK_war_log_clan_CachedClanId",
                        column: x => x.CachedClanId,
                        principalTable: "clan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_clan_ExpiresAt",
                table: "clan",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_clan_Id",
                table: "clan",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clan_KeepUntil",
                table: "clan",
                column: "KeepUntil");

            migrationBuilder.CreateIndex(
                name: "IX_clan_Tag",
                table: "clan",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_current_war_EnemyTag",
                table: "current_war",
                column: "EnemyTag");

            migrationBuilder.CreateIndex(
                name: "IX_current_war_ExpiresAt",
                table: "current_war",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_current_war_KeepUntil",
                table: "current_war",
                column: "KeepUntil");

            migrationBuilder.CreateIndex(
                name: "IX_group_ExpiresAt",
                table: "group",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_group_KeepUntil",
                table: "group",
                column: "KeepUntil");

            migrationBuilder.CreateIndex(
                name: "IX_player_ClanTag",
                table: "player",
                column: "ClanTag");

            migrationBuilder.CreateIndex(
                name: "IX_player_ExpiresAt",
                table: "player",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_player_Id",
                table: "player",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_player_KeepUntil",
                table: "player",
                column: "KeepUntil");

            migrationBuilder.CreateIndex(
                name: "IX_player_Tag",
                table: "player",
                column: "Tag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_war_ClanTag",
                table: "war",
                column: "ClanTag");

            migrationBuilder.CreateIndex(
                name: "IX_war_ExpiresAt",
                table: "war",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_war_Id",
                table: "war",
                column: "Id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_war_IsFinal",
                table: "war",
                column: "IsFinal");

            migrationBuilder.CreateIndex(
                name: "IX_war_KeepUntil",
                table: "war",
                column: "KeepUntil");

            migrationBuilder.CreateIndex(
                name: "IX_war_OpponentTag",
                table: "war",
                column: "OpponentTag");

            migrationBuilder.CreateIndex(
                name: "IX_war_PreparationStartTime_ClanTag_OpponentTag",
                table: "war",
                columns: new[] { "PreparationStartTime", "ClanTag", "OpponentTag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_war_Season",
                table: "war",
                column: "Season");

            migrationBuilder.CreateIndex(
                name: "IX_war_WarTag",
                table: "war",
                column: "WarTag");

            migrationBuilder.CreateIndex(
                name: "IX_war_log_ExpiresAt",
                table: "war_log",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_war_log_KeepUntil",
                table: "war_log",
                column: "KeepUntil");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "current_war");

            migrationBuilder.DropTable(
                name: "group");

            migrationBuilder.DropTable(
                name: "player");

            migrationBuilder.DropTable(
                name: "war");

            migrationBuilder.DropTable(
                name: "war_log");

            migrationBuilder.DropTable(
                name: "clan");
        }
    }
}
