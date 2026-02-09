using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Tier = table.Column<int>(type: "INTEGER", nullable: false),
                    IsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TimeZoneId = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FocusSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TaskId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FocusSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FocusSessions_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Color = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    IsDefault = table.Column<bool>(type: "INTEGER", nullable: false),
                    ViewType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubscriptionSchedules_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaskHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    IsSubTaskName = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UsageCount = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskHistory_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: false),
                    Priority = table.Column<int>(type: "INTEGER", nullable: false),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsFocused = table.Column<bool>(type: "INTEGER", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    ProjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ParentTaskId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    DueDateLocal = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    DueTimeLocal = table.Column<TimeOnly>(type: "TEXT", nullable: true),
                    DueAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsMarkedForToday = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Tasks_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
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
                    SentAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_FocusSessions_SubscriptionId_TaskId_StartedAt",
                table: "FocusSessions",
                columns: new[] { "SubscriptionId", "TaskId", "StartedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MyTaskFlowSections_SubscriptionId_SortOrder",
                table: "MyTaskFlowSections",
                columns: new[] { "SubscriptionId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_MyTaskFlowSectionTasks_TaskId",
                table: "MyTaskFlowSectionTasks",
                column: "TaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SubscriptionId_Name",
                table: "Projects",
                columns: new[] { "SubscriptionId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionSchedules_SubscriptionId_StartsOn",
                table: "SubscriptionSchedules",
                columns: new[] { "SubscriptionId", "StartsOn" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistory_SubscriptionId_IsSubTaskName_Name",
                table: "TaskHistory",
                columns: new[] { "SubscriptionId", "IsSubTaskName", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskHistory_SubscriptionId_LastUsedAt",
                table: "TaskHistory",
                columns: new[] { "SubscriptionId", "LastUsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskReminders_TaskId_TriggerAtUtc_SentAtUtc",
                table: "TaskReminders",
                columns: new[] { "TaskId", "TriggerAtUtc", "SentAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ProjectId",
                table: "Tasks",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SubscriptionId_DueAtUtc",
                table: "Tasks",
                columns: new[] { "SubscriptionId", "DueAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SubscriptionId_DueDateLocal",
                table: "Tasks",
                columns: new[] { "SubscriptionId", "DueDateLocal" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SubscriptionId_ProjectId_CreatedAt",
                table: "Tasks",
                columns: new[] { "SubscriptionId", "ProjectId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SubscriptionId_ProjectId_ParentTaskId_SortOrder",
                table: "Tasks",
                columns: new[] { "SubscriptionId", "ProjectId", "ParentTaskId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FocusSessions");

            migrationBuilder.DropTable(
                name: "MyTaskFlowSectionTasks");

            migrationBuilder.DropTable(
                name: "SubscriptionSchedules");

            migrationBuilder.DropTable(
                name: "TaskHistory");

            migrationBuilder.DropTable(
                name: "TaskReminders");

            migrationBuilder.DropTable(
                name: "MyTaskFlowSections");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Subscriptions");
        }
    }
}
