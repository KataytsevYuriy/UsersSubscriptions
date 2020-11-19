using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UsersSubscriptions.Migrations
{
    public partial class addSchoolPayment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_AspNetUsers_PayedToId",
                table: "Subscriptions");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PayedToId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PayedDatetime",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PayedToId",
                table: "Subscriptions");

            migrationBuilder.RenameColumn(
                name: "Month",
                table: "Subscriptions",
                newName: "Period");

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
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 64, nullable: false),
                    SubscriptionId = table.Column<string>(maxLength: 64, nullable: true),
                    Price = table.Column<int>(nullable: false),
                    DateTime = table.Column<DateTime>(nullable: false),
                    PaymentTypeId = table.Column<string>(maxLength: 64, nullable: true),
                    PayedToId = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_PayedToId",
                        column: x => x.PayedToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "IX_Payments_PayedToId",
                table: "Payments",
                column: "PayedToId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolTransactions_SchoolId",
                table: "SchoolTransactions",
                column: "SchoolId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

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

            migrationBuilder.RenameColumn(
                name: "Period",
                table: "Subscriptions",
                newName: "Month");

            migrationBuilder.AddColumn<DateTime>(
                name: "PayedDatetime",
                table: "Subscriptions",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PayedToId",
                table: "Subscriptions",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PayedToId",
                table: "Subscriptions",
                column: "PayedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_AspNetUsers_PayedToId",
                table: "Subscriptions",
                column: "PayedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
