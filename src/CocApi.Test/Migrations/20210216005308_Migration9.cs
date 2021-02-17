using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CocApi.Test.Migrations
{
    public partial class Migration9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "current_war",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PreparationStartTime",
                table: "current_war",
                type: "timestamp without time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "current_war",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "PreparationStartTime",
                table: "current_war",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone",
                oldNullable: true);
        }
    }
}
