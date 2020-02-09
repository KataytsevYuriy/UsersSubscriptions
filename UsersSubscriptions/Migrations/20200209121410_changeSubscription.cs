using Microsoft.EntityFrameworkCore.Migrations;

namespace UsersSubscriptions.Migrations
{
    public partial class changeSubscription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedbyTeacher",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PyedToTeacher",
                table: "Subscriptions");

            migrationBuilder.CreateTable(
                name: "SubscriptionCreatedby",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 64, nullable: false),
                    SubscriptionId = table.Column<string>(maxLength: 64, nullable: true),
                    AppUserId = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionCreatedby", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionCreatedby_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionCreatedby_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionPayedTo",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 64, nullable: false),
                    SubscriptionId = table.Column<string>(maxLength: 64, nullable: true),
                    AppUserId = table.Column<string>(maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionPayedTo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayedTo_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionPayedTo_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionCreatedby_AppUserId",
                table: "SubscriptionCreatedby",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionCreatedby_SubscriptionId",
                table: "SubscriptionCreatedby",
                column: "SubscriptionId",
                unique: true,
                filter: "[SubscriptionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayedTo_AppUserId",
                table: "SubscriptionPayedTo",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionPayedTo_SubscriptionId",
                table: "SubscriptionPayedTo",
                column: "SubscriptionId",
                unique: true,
                filter: "[SubscriptionId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionCreatedby");

            migrationBuilder.DropTable(
                name: "SubscriptionPayedTo");

            migrationBuilder.AddColumn<string>(
                name: "CreatedbyTeacher",
                table: "Subscriptions",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PyedToTeacher",
                table: "Subscriptions",
                maxLength: 64,
                nullable: true);
        }
    }
}
