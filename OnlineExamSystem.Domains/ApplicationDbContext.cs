using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.Domains.Entities;

namespace OnlineExamSystem.Domains
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<Exam> Exams { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Choice> Choices { get; set; }

        public DbSet<ExamSubmission> ExamSubmissions { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        // public DbSet<UserExam> UserExams { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure identity configurations are applied

            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });

            modelBuilder.Entity<Exam>()
                .HasMany(e => e.Questions)
                .WithOne(q => q.Exam)
                .HasForeignKey(q => q.ExamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasMany(q => q.Choices)
                .WithOne(c => c.Question)
                .HasForeignKey(c => c.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Question>()
                .HasOne(q => q.CorrectChoice)
                .WithOne()
                .HasForeignKey<Question>(q => q.CorrectChoiceId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // -----------------------------

            modelBuilder.Entity<ExamSubmission>()
           .HasKey(es => es.SubmissionId);

            modelBuilder.Entity<ExamSubmission>()
                .HasOne(es => es.User)
                .WithMany()
                .HasForeignKey(es => es.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade from User

            modelBuilder.Entity<ExamSubmission>()
                .HasOne(es => es.Exam)
                .WithMany()
                .HasForeignKey(es => es.ExamId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade from Exam

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Submission)
                .WithMany(es => es.Answers)
                .HasForeignKey(ua => ua.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade); // Allow cascade from Submission

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.Question)
                .WithMany()
                .HasForeignKey(ua => ua.QuestionId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade from Question

            modelBuilder.Entity<UserAnswer>()
                .HasOne(ua => ua.SelectedChoice)
                .WithMany()
                .HasForeignKey(ua => ua.SelectedChoiceId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade from Choice

            // Use static GUIDs instead of Guid.NewGuid()
            var adminRoleId = "8a3b5d7c-fb0b-42c9-a5c2-bd055b43a6c4";
            var userRoleId = "d4c1fa52-9a2e-47b6-9cb1-34a6d612c8e7";

            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = userRoleId, Name = "User", NormalizedName = "USER" });







        }
    }
}
