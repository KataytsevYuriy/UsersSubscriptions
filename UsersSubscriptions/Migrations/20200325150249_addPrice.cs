using Microsoft.EntityFrameworkCore.Migrations;

namespace UsersSubscriptions.Migrations
{
    public partial class addPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Subscriptions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Courses",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Courses");
        }
    }
}
