using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMyTaskFlowSlices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DueAtUtc",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateOnly>(
                name: "DueDateLocal",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DueTimeLocal",
                table: "Tasks",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "HasDueDate",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasDueTime",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMarkedForToday",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TimeZoneId",
                table: "Subscriptions",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "MyTaskFlowSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsSystemSection = table.Column<bool>(type: "INTEGER", nullable: false),
                    DueBucket = table.Column<int>(type: "INTEGER", nullable: false),
                    IncludeAssignedTasks = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncludeUnassignedTasks = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncludeDoneTasks = table.Column<bool>(type: "INTEGER", nullable: false),
                    IncludeCancelledTasks = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyTaskFlowSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MyTaskFlowSections_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Mode = table.Column<int>(type: "INTEGER", nullable: false),
                    MinutesBefore = table.Column<int>(type: "INTEGER", nullable: false),
                    FallbackLocalTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    TriggerAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SentAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskReminders_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MyTaskFlowSectionTasks",
                columns: table => new
                {
                    SectionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MyTaskFlowSectionTasks", x => new { x.SectionId, x.TaskId });
                    table.ForeignKey(
                        name: "FK_MyTaskFlowSectionTasks_MyTaskFlowSections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "MyTaskFlowSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MyTaskFlowSectionTasks_Tasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SubscriptionId_DueAtUtc",
                table: "Tasks",
                columns: new[] { "SubscriptionId", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SubscriptionId_DueDateLocal",
                table: "Tasks",
                columns: new[] { "SubscriptionId", "DueDateLocal" });

            migrationBuilder.CreateIndex(
                name: "IX_MyTaskFlowSections_SubscriptionId_SortOrder",
                table: "MyTaskFlowSections",
                columns: new[] { "SubscriptionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MyTaskFlowSectionTasks_TaskId",
                table: "MyTaskFlowSectionTasks",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminders_TaskId_TriggerAtUtc_SentAtUtc",
                table: "TaskReminders",
                columns: new[] { "TaskId", "TriggerAtUtc", "SentAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MyTaskFlowSectionTasks");

            migrationBuilder.DropTable(
                name: "TaskReminders");

            migrationBuilder.DropTable(
                name: "MyTaskFlowSections");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_SubscriptionId_DueAtUtc",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_SubscriptionId_DueDateLocal",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DueAtUtc",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DueDateLocal",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DueTimeLocal",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "HasDueDate",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "HasDueTime",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "IsMarkedForToday",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "TimeZoneId",
                table: "Subscriptions");
        }
    }
}
