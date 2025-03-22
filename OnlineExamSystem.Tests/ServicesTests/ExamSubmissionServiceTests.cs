using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.BL.Services;
using OnlineExamSystem.BL.UnitOfWork;
using OnlineExamSystem.Domains;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Tests.Helpers;
using Xunit;

namespace OnlineExamSystem.Tests.ServicesTests
{
    public class ExamSubmissionServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly ExamSubmissionService _submissionService;

        public ExamSubmissionServiceTests()
        {
            _context = TestDbContext.Create();
            _context.Database.EnsureDeleted(); // Reset the database before each test
            _context.Database.EnsureCreated();
            _unitOfWork = new UnitOfWork(_context);
            _submissionService = new ExamSubmissionService(_unitOfWork);

            // Seed initial data
            SeedData();
        }

        private void SeedData()
        {
            var exam = new Exam
            {
                ExamId = 1,
                Title = "Math Exam",
                CreatedDate = DateTime.Now,
                CreatedBy = "Admin",
                Questions = new List<Question>
                {
                    new Question
                    {
                        QuestionId = 1,
                        Title = "What is 2+2?",
                        ExamId = 1,
                        Choices = new List<Choice>
                        {
                            new Choice { ChoiceId = 1, Text = "3", IsCorrect = false },
                            new Choice { ChoiceId = 2, Text = "4", IsCorrect = true },
                            new Choice { ChoiceId = 3, Text = "5", IsCorrect = false },
                            new Choice { ChoiceId = 4, Text = "6", IsCorrect = false }
                        },
                        CorrectChoiceId = 2
                    },
                    new Question
                    {
                        QuestionId = 2,
                        Title = "What is 3+3?",
                        ExamId = 1,
                        Choices = new List<Choice>
                        {
                            new Choice { ChoiceId = 5, Text = "6", IsCorrect = true },
                            new Choice { ChoiceId = 6, Text = "7", IsCorrect = false },
                            new Choice { ChoiceId = 7, Text = "8", IsCorrect = false },
                            new Choice { ChoiceId = 8, Text = "9", IsCorrect = false }
                        },
                        CorrectChoiceId = 5
                    }
                }
            };

            var user = new ApplicationUser
            {
                Id = "user1",
                UserName = "testuser"
            };

            _context.Users.Add(user);
            _context.Exams.Add(exam);
            _context.SaveChanges();
        }

        [Fact]
        public async Task GetUserSubmissionsAsync_ReturnsUserSubmissions()
        {
            // Arrange
            var submission = new ExamSubmission
            {
                UserId = "user1",
                ExamId = 1,
                SubmissionDate = DateTime.UtcNow,
                TotalQuestions = 2,
                CorrectAnswers = 1,
                Score = 50,
                IsPassed = true
            };
            await _unitOfWork.Repository<ExamSubmission>().AddAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            // Act
            var submissions = await _submissionService.GetUserSubmissionsAsync("user1");

            // Assert
            Assert.NotNull(submissions);
            Assert.Single(submissions);
            Assert.Equal("user1", submissions[0].UserId);
            Assert.Equal(1, submissions[0].ExamId);
        }

        [Fact]
        public async Task GetUserSubmissionsAsync_NoSubmissions_ReturnsEmptyList()
        {
            // Act
            var submissions = await _submissionService.GetUserSubmissionsAsync("user2");

            // Assert
            Assert.NotNull(submissions);
            Assert.Empty(submissions);
        }

        [Fact]
        public async Task SubmitExamAsync_SuccessfulSubmission_ReturnsSubmission()
        {
            // Arrange
            var userId = "user1";
            var examId = 1;
            var answers = new Dictionary<int, int>
            {
                { 1, 2 }, // Correct (Question 1, Choice 2 is correct)
                { 2, 6 }  // Incorrect (Question 2, Choice 5 is correct)
            };

            // Act
            var submission = await _submissionService.SubmitExamAsync(userId, examId, answers);

            // Assert
            Assert.NotNull(submission);
            Assert.Equal(userId, submission.UserId);
            Assert.Equal(examId, submission.ExamId);
            Assert.Equal(2, submission.TotalQuestions);
            Assert.Equal(1, submission.CorrectAnswers);
            Assert.Equal(50, submission.Score); // 1 out of 2 correct = 50%
            Assert.True(submission.IsPassed); // 50% is passing threshold
            Assert.Equal(2, submission.Answers.Count);

            // Verify in database
            var dbSubmission = await _context.ExamSubmissions
                .Include(es => es.Answers)
                .FirstOrDefaultAsync(es => es.SubmissionId == submission.SubmissionId);
            Assert.NotNull(dbSubmission);
            Assert.Equal(2, dbSubmission.Answers.Count);
        }

        [Fact]
        public async Task SubmitExamAsync_ExamNotFound_ThrowsException()
        {
            // Arrange
            var userId = "user1";
            var examId = 999; // Non-existent exam
            var answers = new Dictionary<int, int> { { 1, 2 } };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
                await _submissionService.SubmitExamAsync(userId, examId, answers));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}