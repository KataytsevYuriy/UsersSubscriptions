using Microsoft.EntityFrameworkCore.Migrations;

namespace UsersSubscriptions.Migrations
{
    public partial class addDbSetCoursAppUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseAppUser_AspNetUsers_AppUserId",
                table: "CourseAppUser");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseAppUser_Courses_CourseId",
                table: "CourseAppUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseAppUser",
                table: "CourseAppUser");

            migrationBuilder.RenameTable(
                name: "CourseAppUser",
                newName: "CourseAppUsers");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAppUser_CourseId",
                table: "CourseAppUsers",
                newName: "IX_courseAppUsers_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAppUser_AppUserId",
                table: "CourseAppUsers",
                newName: "IX_courseAppUsers_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_courseAppUsers",
                table: "CourseAppUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_courseAppUsers_AspNetUsers_AppUserId",
                table: "CourseAppUsers",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_courseAppUsers_Courses_CourseId",
                table: "CourseAppUsers",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courseAppUsers_AspNetUsers_AppUserId",
                table: "CourseAppUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_courseAppUsers_Courses_CourseId",
                table: "CourseAppUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_courseAppUsers",
                table: "CourseAppUsers");

            migrationBuilder.RenameTable(
                name: "CourseAppUsers",
                newName: "CourseAppUser");

            migrationBuilder.RenameIndex(
                name: "IX_courseAppUsers_CourseId",
                table: "CourseAppUser",
                newName: "IX_CourseAppUser_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_courseAppUsers_AppUserId",
                table: "CourseAppUser",
                newName: "IX_CourseAppUser_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseAppUser",
                table: "CourseAppUser",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAppUser_AspNetUsers_AppUserId",
                table: "CourseAppUser",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAppUser_Courses_CourseId",
                table: "CourseAppUser",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
