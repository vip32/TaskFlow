using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImportantMyTaskFlowSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"UPDATE MyTaskFlowSections
                    SET DueBucket = 6,
                        IsSystemSection = 1,
                        SortOrder = 2
                    WHERE lower(Name) = 'important';");

            migrationBuilder.Sql(
                @"INSERT INTO MyTaskFlowSections (
                        Id,
                        SubscriptionId,
                        Name,
                        SortOrder,
                        IsSystemSection,
                        DueBucket,
                        IncludeAssignedTasks,
                        IncludeUnassignedTasks,
                        IncludeDoneTasks,
                        IncludeCancelledTasks)
                    SELECT
                        lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-' || lower(hex(randomblob(2))) || '-' || lower(hex(randomblob(2))) || '-' || lower(hex(randomblob(6))),
                        s.Id,
                        'Important',
                        2,
                        1,
                        6,
                        1,
                        1,
                        0,
                        0
                    FROM Subscriptions s
                    WHERE NOT EXISTS (
                        SELECT 1
                        FROM MyTaskFlowSections existing
                        WHERE existing.SubscriptionId = s.Id
                          AND existing.DueBucket = 6);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"DELETE FROM MyTaskFlowSections
                    WHERE DueBucket = 6
                      AND IsSystemSection = 1
                      AND Name = 'Important';");
        }
    }
}
