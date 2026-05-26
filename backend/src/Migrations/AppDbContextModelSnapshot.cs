using HackBackend.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace HackBackend.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder b)
    {
#pragma warning disable 612, 618
        b.HasAnnotation("ProductVersion", "8.0.11")
         .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(b);

        b.Entity("HackBackend.Models.User", e =>
        {
            e.Property<int>("Id").ValueGeneratedOnAdd()
             .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            e.Property<string>("Name").IsRequired();
            e.Property<string>("Email").IsRequired();
            e.Property<string>("PasswordHash").IsRequired();
            e.Property<string>("Role").IsRequired();
            e.Property<string?>("VkUserId");
            e.Property<DateTime>("CreatedAt");
            e.HasKey("Id");
            e.HasIndex("Email").IsUnique();
            e.ToTable("Users");
        });

        b.Entity("HackBackend.Models.Homework", e =>
        {
            e.Property<int>("Id").ValueGeneratedOnAdd()
             .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            e.Property<string>("Title").IsRequired();
            e.Property<string?>("Description");
            e.Property<string>("Project").IsRequired();
            e.Property<string>("Direction").IsRequired();
            e.Property<int>("Course");
            e.Property<int>("CreatedBy");
            e.Property<DateTime?>("Deadline");
            e.Property<DateTime>("CreatedAt");
            e.HasKey("Id");
            e.ToTable("Homeworks");
        });

        b.Entity("HackBackend.Models.Submission", e =>
        {
            e.Property<int>("Id").ValueGeneratedOnAdd()
             .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            e.Property<int>("HomeworkId");
            e.Property<int>("StudentId");
            e.Property<string>("Status").IsRequired();
            e.Property<DateTime>("SubmittedAt");
            e.Property<DateTime>("CreatedAt");
            e.HasKey("Id");
            e.ToTable("Submissions");
        });

        b.Entity("HackBackend.Models.SubmissionItem", e =>
        {
            e.Property<int>("Id").ValueGeneratedOnAdd()
             .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            e.Property<int>("SubmissionId");
            e.Property<string>("Type").IsRequired();
            e.Property<string?>("Url");
            e.Property<string?>("FilePath");
            e.Property<string?>("OriginalName");
            e.Property<string?>("MimeType");
            e.Property<long?>("FileSize");
            e.Property<DateTime>("CreatedAt");
            e.HasKey("Id");
            e.ToTable("SubmissionItems");
        });

        b.Entity("HackBackend.Models.Grade", e =>
        {
            e.Property<int>("Id").ValueGeneratedOnAdd()
             .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            e.Property<int>("SubmissionId");
            e.Property<int>("TeacherId");
            e.Property<int>("Score");
            e.Property<string?>("Comment");
            e.Property<DateTime>("GradedAt");
            e.HasKey("Id");
            e.HasAlternateKey("SubmissionId");
            e.ToTable("Grades");
        });

        b.Entity("HackBackend.Models.VkDeadlineReminderLog", e =>
        {
            e.Property<int>("Id").ValueGeneratedOnAdd()
             .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            e.Property<int>("StudentId");
            e.Property<int>("HomeworkId");
            e.Property<string>("Kind").IsRequired().HasMaxLength(32);
            e.Property<DateTime>("SentAt");
            e.HasKey("Id");
            e.HasIndex("StudentId", "HomeworkId", "Kind").IsUnique();
            e.ToTable("VkDeadlineReminderLogs");
        });

        b.Entity("HackBackend.Models.Homework", e =>
        {
            e.HasOne("HackBackend.Models.User", "Author")
             .WithMany("CreatedHomeworks")
             .HasForeignKey("CreatedBy");
        });

        b.Entity("HackBackend.Models.Submission", e =>
        {
            e.HasOne("HackBackend.Models.Homework", "Homework")
             .WithMany("Submissions")
             .HasForeignKey("HomeworkId");
            e.HasOne("HackBackend.Models.User", "Student")
             .WithMany("Submissions")
             .HasForeignKey("StudentId");
        });

        b.Entity("HackBackend.Models.SubmissionItem", e =>
        {
            e.HasOne("HackBackend.Models.Submission", "Submission")
             .WithMany("Items")
             .HasForeignKey("SubmissionId");
        });

        b.Entity("HackBackend.Models.Grade", e =>
        {
            e.HasOne("HackBackend.Models.Submission", "Submission")
             .WithOne("Grade")
             .HasForeignKey("HackBackend.Models.Grade", "SubmissionId");
            e.HasOne("HackBackend.Models.User", "Teacher")
             .WithMany("GradesGiven")
             .HasForeignKey("TeacherId");
        });

        b.Entity("HackBackend.Models.VkDeadlineReminderLog", e =>
        {
            e.HasOne("HackBackend.Models.User", "Student")
             .WithMany()
             .HasForeignKey("StudentId")
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne("HackBackend.Models.Homework", "Homework")
             .WithMany()
             .HasForeignKey("HomeworkId")
             .OnDelete(DeleteBehavior.Cascade);
        });
#pragma warning restore 612, 618
    }
}
