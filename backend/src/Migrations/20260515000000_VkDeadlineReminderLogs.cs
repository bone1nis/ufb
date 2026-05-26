using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HackBackend.Migrations;

[DbContext(typeof(HackBackend.Data.AppDbContext))]
[Migration("20260515000000_VkDeadlineReminderLogs")]
public partial class VkDeadlineReminderLogs : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.CreateTable(
            name: "VkDeadlineReminderLogs",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                StudentId = table.Column<int>(nullable: false),
                HomeworkId = table.Column<int>(nullable: false),
                Kind = table.Column<string>(maxLength: 32, nullable: false),
                SentAt = table.Column<DateTime>(nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VkDeadlineReminderLogs", x => x.Id);
                table.ForeignKey(
                    name: "FK_VkDeadlineReminderLogs_Homeworks_HomeworkId",
                    column: x => x.HomeworkId,
                    principalTable: "Homeworks",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_VkDeadlineReminderLogs_Users_StudentId",
                    column: x => x.StudentId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        m.CreateIndex(
            name: "IX_VkDeadlineReminderLogs_StudentId_HomeworkId_Kind",
            table: "VkDeadlineReminderLogs",
            columns: new[] { "StudentId", "HomeworkId", "Kind" },
            unique: true);
    }

    protected override void Down(MigrationBuilder m)
    {
        m.DropTable(name: "VkDeadlineReminderLogs");
    }
}
