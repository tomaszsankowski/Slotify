using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectDefense.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDeleteForReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_StudentId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_SupervisorAvailabilities_SupervisorAvailabilityId",
                table: "Reservations");

            migrationBuilder.AlterColumn<string>(
                name: "RoomNumber",
                table: "Rooms",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Rooms",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_StudentId",
                table: "Reservations",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_SupervisorAvailabilities_SupervisorAvailabilityId",
                table: "Reservations",
                column: "SupervisorAvailabilityId",
                principalTable: "SupervisorAvailabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_StudentId",
                table: "Reservations");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_SupervisorAvailabilities_SupervisorAvailabilityId",
                table: "Reservations");

            migrationBuilder.AlterColumn<string>(
                name: "RoomNumber",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Rooms",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_StudentId",
                table: "Reservations",
                column: "StudentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_SupervisorAvailabilities_SupervisorAvailabilityId",
                table: "Reservations",
                column: "SupervisorAvailabilityId",
                principalTable: "SupervisorAvailabilities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
