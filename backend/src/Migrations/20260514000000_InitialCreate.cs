using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HackBackend.Migrations;

[DbContext(typeof(HackBackend.Data.AppDbContext))]
[Migration("20260514000000_InitialCreate")]
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder m)
    {
        m.CreateTable("Users",
            t => new
            {
                Id           = t.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Name         = t.Column<string>(nullable: false),
                Email        = t.Column<string>(nullable: false),
                PasswordHash = t.Column<string>(nullable: false),
                Role         = t.Column<string>(nullable: false),
                VkUserId     = t.Column<string>(nullable: true),
                CreatedAt    = t.Column<DateTime>(nullable: false),
            },
            constraints: t => t.PrimaryKey("PK_Users", x => x.Id));

        m.CreateIndex("IX_Users_Email", "Users", "Email", unique: true);

        m.CreateTable("Homeworks",
            t => new
            {
                Id          = t.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Title       = t.Column<string>(nullable: false),
                Description = t.Column<string>(nullable: true),
                Project     = t.Column<string>(nullable: false),
                Direction   = t.Column<string>(nullable: false),
                Course      = t.Column<int>(nullable: false),
                CreatedBy   = t.Column<int>(nullable: false),
                Deadline    = t.Column<DateTime>(nullable: true),
                CreatedAt   = t.Column<DateTime>(nullable: false),
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Homeworks", x => x.Id);
                t.ForeignKey("FK_Homeworks_Users_CreatedBy", x => x.CreatedBy, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

        m.CreateTable("Submissions",
            t => new
            {
                Id          = t.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                HomeworkId  = t.Column<int>(nullable: false),
                StudentId   = t.Column<int>(nullable: false),
                Status      = t.Column<string>(nullable: false),
                SubmittedAt = t.Column<DateTime>(nullable: false),
                CreatedAt   = t.Column<DateTime>(nullable: false),
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Submissions", x => x.Id);
                t.ForeignKey("FK_Submissions_Homeworks_HomeworkId", x => x.HomeworkId, "Homeworks", "Id", onDelete: ReferentialAction.Cascade);
                t.ForeignKey("FK_Submissions_Users_StudentId", x => x.StudentId, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

        m.CreateTable("SubmissionItems",
            t => new
            {
                Id           = t.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SubmissionId = t.Column<int>(nullable: false),
                Type         = t.Column<string>(nullable: false),
                Url          = t.Column<string>(nullable: true),
                FilePath     = t.Column<string>(nullable: true),
                OriginalName = t.Column<string>(nullable: true),
                MimeType     = t.Column<string>(nullable: true),
                FileSize     = t.Column<long>(nullable: true),
                CreatedAt    = t.Column<DateTime>(nullable: false),
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_SubmissionItems", x => x.Id);
                t.ForeignKey("FK_SubmissionItems_Submissions_SubmissionId", x => x.SubmissionId, "Submissions", "Id", onDelete: ReferentialAction.Cascade);
            });

        m.CreateTable("Grades",
            t => new
            {
                Id           = t.Column<int>(nullable: false).Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                SubmissionId = t.Column<int>(nullable: false),
                TeacherId    = t.Column<int>(nullable: false),
                Score        = t.Column<int>(nullable: false),
                Comment      = t.Column<string>(nullable: true),
                GradedAt     = t.Column<DateTime>(nullable: false),
            },
            constraints: t =>
            {
                t.PrimaryKey("PK_Grades", x => x.Id);
                t.UniqueConstraint("AK_Grades_SubmissionId", x => x.SubmissionId);
                t.ForeignKey("FK_Grades_Submissions_SubmissionId", x => x.SubmissionId, "Submissions", "Id", onDelete: ReferentialAction.Cascade);
                t.ForeignKey("FK_Grades_Users_TeacherId", x => x.TeacherId, "Users", "Id", onDelete: ReferentialAction.Cascade);
            });
    }

    protected override void Down(MigrationBuilder m)
    {
        m.DropTable("Grades");
        m.DropTable("SubmissionItems");
        m.DropTable("Submissions");
        m.DropTable("Homeworks");
        m.DropTable("Users");
    }
}
