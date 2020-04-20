using Microsoft.EntityFrameworkCore.Migrations;

namespace UsersSubscriptions.Migrations
{
    public partial class AddUrlNameToSchool2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UrlName",
                table: "Schools",
                maxLength: 16,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Schools_UrlName",
                table: "Schools",
                column: "UrlName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Schools_UrlName",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "UrlName",
                table: "Schools");
        }
    }
}
