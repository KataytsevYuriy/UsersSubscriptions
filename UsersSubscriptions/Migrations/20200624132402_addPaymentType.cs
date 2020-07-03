using Microsoft.EntityFrameworkCore.Migrations;

namespace UsersSubscriptions.Migrations
{
    public partial class addPaymentType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comment",
                table: "Subscriptions",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTypeId",
                table: "Subscriptions",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 64, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Priority = table.Column<int>(nullable: false),
                    SchoolId = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentTypes_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CoursePaymentTypes",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 64, nullable: false),
                    CourseId = table.Column<string>(maxLength: 64, nullable: true),
                    PaymentTypeId = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoursePaymentTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CoursePaymentTypes_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CoursePaymentTypes_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PaymentTypeId",
                table: "Subscriptions",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePaymentTypes_CourseId",
                table: "CoursePaymentTypes",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CoursePaymentTypes_PaymentTypeId",
                table: "CoursePaymentTypes",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_SchoolId",
                table: "PaymentTypes",
                column: "SchoolId");

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_PaymentTypes_PaymentTypeId",
                table: "Subscriptions",
                column: "PaymentTypeId",
                principalTable: "PaymentTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_PaymentTypes_PaymentTypeId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "CoursePaymentTypes");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Subscriptions_PaymentTypeId",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "Comment",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PaymentTypeId",
                table: "Subscriptions");
        }
    }
}
