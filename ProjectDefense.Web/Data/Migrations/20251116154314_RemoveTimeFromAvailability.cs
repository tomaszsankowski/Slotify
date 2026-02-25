using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectDefense.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTimeFromAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndTime",
                table: "SupervisorAvailabilities");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "SupervisorAvailabilities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeSpan>(
                name: "EndTime",
                table: "SupervisorAvailabilities",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                table: "SupervisorAvailabilities",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }
    }
}
