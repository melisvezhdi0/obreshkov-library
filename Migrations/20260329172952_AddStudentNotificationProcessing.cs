using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObreshkovLibrary.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentNotificationProcessing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastOverdueReminderSentOn",
                table: "Loans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Reminder1DaySent",
                table: "Loans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Reminder3DaysSent",
                table: "Loans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Reminder7DaysSent",
                table: "Loans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedOn",
                table: "BookAvailabilityRequests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastOverdueReminderSentOn",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "Reminder1DaySent",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "Reminder3DaysSent",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "Reminder7DaysSent",
                table: "Loans");

            migrationBuilder.DropColumn(
                name: "DeactivatedOn",
                table: "BookAvailabilityRequests");
        }
    }
}
