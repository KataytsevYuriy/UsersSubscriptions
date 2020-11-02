using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UsersSubscriptions.Migrations
{
    public partial class addSchoolPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AllowTestUntil",
                table: "Schools",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Balance",
                table: "Schools",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Enable",
                table: "Schools",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPayed",
                table: "Schools",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PayedMonth",
                table: "Schools",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "Schools",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SchoolTransactions",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 64, nullable: false),
                    Payed = table.Column<int>(nullable: false),
                    OldBalance = table.Column<int>(nullable: false),
                    NewBalance = table.Column<int>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    PayedDateTime = table.Column<DateTime>(nullable: false),
                    SchoolId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolTransactions_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolTransactions_SchoolId",
                table: "SchoolTransactions",
                column: "SchoolId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolTransactions");

            migrationBuilder.DropColumn(
                name: "AllowTestUntil",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Enable",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "IsPayed",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "PayedMonth",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Schools");
        }
    }
}
