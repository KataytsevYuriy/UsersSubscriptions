using Microsoft.EntityFrameworkCore.Migrations;

namespace UsersSubscriptions.Migrations
{
    public partial class addOneTimeSubscriptions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "Subscriptions",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "MonthSubscription",
                table: "Subscriptions",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Subscriptions",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowOneTimePrice",
                table: "Courses",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "OneTimePrice",
                table: "Courses",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "MonthSubscription",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "AllowOneTimePrice",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "OneTimePrice",
                table: "Courses");
        }
    }
}
