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
                newName: "courseAppUsers");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAppUser_CourseId",
                table: "courseAppUsers",
                newName: "IX_courseAppUsers_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAppUser_AppUserId",
                table: "courseAppUsers",
                newName: "IX_courseAppUsers_AppUserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_courseAppUsers",
                table: "courseAppUsers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_courseAppUsers_AspNetUsers_AppUserId",
                table: "courseAppUsers",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_courseAppUsers_Courses_CourseId",
                table: "courseAppUsers",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courseAppUsers_AspNetUsers_AppUserId",
                table: "courseAppUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_courseAppUsers_Courses_CourseId",
                table: "courseAppUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_courseAppUsers",
                table: "courseAppUsers");

            migrationBuilder.RenameTable(
                name: "courseAppUsers",
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
