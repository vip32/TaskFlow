using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriptionSettings",
                columns: table => new
                {
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AlwaysShowCompletedTasks = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionSettings", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_SubscriptionSettings_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
                @"INSERT INTO SubscriptionSettings (SubscriptionId, AlwaysShowCompletedTasks)
                  SELECT s.Id, 0
                  FROM Subscriptions s
                  WHERE NOT EXISTS (
                      SELECT 1
                      FROM SubscriptionSettings existing
                      WHERE existing.SubscriptionId = s.Id
                  );");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriptionSettings");
        }
    }
}
