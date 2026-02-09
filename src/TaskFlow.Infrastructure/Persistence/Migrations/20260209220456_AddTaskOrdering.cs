using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskOrdering : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                table: "Tasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_SubscriptionId_ProjectId_ParentTaskId_SortOrder",
                table: "Tasks",
                columns: new[] { "SubscriptionId", "ProjectId", "ParentTaskId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Tasks_SubscriptionId_ProjectId_ParentTaskId_SortOrder",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "Tasks");
        }
    }
}
