using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CrunchyRolls.Data.Migrations
{
    /// <inheritdoc />
    public partial class NoOp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 4, 18, 34, 39, 479, DateTimeKind.Utc).AddTicks(9911),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 1, 4, 18, 32, 34, 861, DateTimeKind.Utc).AddTicks(4002));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedDate",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(2026, 1, 4, 18, 32, 34, 861, DateTimeKind.Utc).AddTicks(4002),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldDefaultValue: new DateTime(2026, 1, 4, 18, 34, 39, 479, DateTimeKind.Utc).AddTicks(9911));
        }
    }
}
