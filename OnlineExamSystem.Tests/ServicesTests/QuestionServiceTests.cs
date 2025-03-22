using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.BL.Services;
using OnlineExamSystem.BL.UnitOfWork;
using OnlineExamSystem.Domains;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Tests.Helpers;
using Xunit;

namespace OnlineExamSystem.Tests.ServicesTests
{
    public class QuestionServiceTests
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;
        private readonly QuestionService _questionService;

        public QuestionServiceTests()
        {
            _context = TestDbContext.Create();
            _unitOfWork = new UnitOfWork(_context);
            _questionService = new QuestionService(_unitOfWork);

            // Seed initial data
            SeedData();
        }

        private void SeedData()
        {
            var exam = new Exam
            {
                ExamId = 1,
                Title = "Test Exam",
                CreatedDate = DateTime.Now,
                CreatedBy = "Admin"
            };
            _context.Exams.Add(exam);
            _context.SaveChanges();
        }

        [Fact]
        public async Task AddQuestionAsync_SuccessfulAddition_ReturnsTrueAndQuestion()
        {
            // Arrange
            var question = new Question
            {
                Title = "What is 2+2?",
                ExamId = 1
            };
            var choices = new List<Choice>
    {
        new Choice { Text = "3", IsCorrect = false },
        new Choice { Text = "4", IsCorrect = true },
        new Choice { Text = "5", IsCorrect = false },
        new Choice { Text = "6", IsCorrect = false }
    };

            // Act
            var (success, addedQuestion) = await _questionService.AddQuestionAsync(question, choices);

            // Assert
            Assert.True(success);
            Assert.NotNull(addedQuestion);
            Assert.Equal("What is 2+2?", addedQuestion.Title);
            Assert.Equal(1, addedQuestion.ExamId);
            Assert.Equal(4, addedQuestion.Choices.Count);
            Assert.NotNull(addedQuestion.CorrectChoiceId);

            // Verify in database
            var dbQuestion = await _context.Questions
                .Include(q => q.Choices)
                .FirstOrDefaultAsync(q => q.QuestionId == addedQuestion.QuestionId);
            Assert.NotNull(dbQuestion);
            Assert.Equal(4, dbQuestion.Choices.Count);

            // Verify CorrectChoiceId by checking the database
            var correctChoice = dbQuestion.Choices.First(c => c.IsCorrect);
            Assert.Equal(correctChoice.ChoiceId, addedQuestion.CorrectChoiceId);
            Assert.Equal("4", correctChoice.Text); // Ensure the correct choice has the expected text
        }
        [Fact]
        public async Task AddQuestionAsync_ExamNotFound_ReturnsFalse()
        {
            // Arrange
            var question = new Question
            {
                Title = "What is 3+3?",
                ExamId = 999 // Non-existent exam
            };
            var choices = new List<Choice>
            {
                new Choice { Text = "6", IsCorrect = true },
                new Choice { Text = "7", IsCorrect = false },
                new Choice { Text = "8", IsCorrect = false },
                new Choice { Text = "9", IsCorrect = false }
            };

            // Act
            var (success, addedQuestion) = await _questionService.AddQuestionAsync(question, choices);

            // Assert
            Assert.False(success);
            Assert.Null(addedQuestion);
        }

        [Fact]
        public async Task EditQuestionAsync_SuccessfulUpdate_ReturnsTrueAndUpdatedQuestion()
        {
            // Arrange
            var question = new Question
            {
                Title = "What is 2+2?",
                ExamId = 1
            };
            var choices = new List<Choice>
    {
        new Choice { Text = "3", IsCorrect = false },
        new Choice { Text = "4", IsCorrect = true },
        new Choice { Text = "5", IsCorrect = false },
        new Choice { Text = "6", IsCorrect = false }
    };
            var (addSuccess, addedQuestion) = await _questionService.AddQuestionAsync(question, choices);
            Assert.True(addSuccess);

            // Modify question
            addedQuestion.Title = "What is 3+3?";
            var updatedChoices = addedQuestion.Choices.Select((c, index) => new Choice
            {
                ChoiceId = c.ChoiceId,
                Text = (index + 6).ToString(), // Update to "6", "7", "8", "9"
                IsCorrect = index == 0 // Set first choice as correct
            }).ToList();

            // Act
            var (editSuccess, editedQuestion) = await _questionService.EditQuestionAsync(addedQuestion, updatedChoices);

            // Assert
            Assert.True(editSuccess);
            Assert.NotNull(editedQuestion);
            Assert.Equal("What is 3+3?", editedQuestion.Title);
            Assert.Equal(4, editedQuestion.Choices.Count);
            Assert.Equal(1, editedQuestion.CorrectChoiceId); // First choice ID should be 1 after edit

            // Verify in database
            var dbQuestion = await _context.Questions
                .Include(q => q.Choices)
                .FirstOrDefaultAsync(q => q.QuestionId == editedQuestion.QuestionId);
            Assert.NotNull(dbQuestion);
            Assert.Equal("6", dbQuestion.Choices.First(c => c.IsCorrect).Text);
        }

        [Fact]
        public async Task EditQuestionAsync_QuestionNotFound_ReturnsFalse()
        {
            // Arrange
            var question = new Question
            {
                QuestionId = 999, // Non-existent question
                Title = "Non-existent",
                ExamId = 1
            };
            var choices = new List<Choice>
            {
                new Choice { Text = "A", IsCorrect = true },
                new Choice { Text = "B", IsCorrect = false },
                new Choice { Text = "C", IsCorrect = false },
                new Choice { Text = "D", IsCorrect = false }
            };

            // Act
            var (success, editedQuestion) = await _questionService.EditQuestionAsync(question, choices);

            // Assert
            Assert.False(success);
            Assert.Null(editedQuestion);
        }

        [Fact]
        public async Task DeleteQuestionAsync_SuccessfulDeletion_ReturnsTrue()
        {
            // Arrange
            var question = new Question
            {
                Title = "What is 2+2?",
                ExamId = 1
            };
            var choices = new List<Choice>
    {
        new Choice { Text = "3", IsCorrect = false },
        new Choice { Text = "4", IsCorrect = true },
        new Choice { Text = "5", IsCorrect = false },
        new Choice { Text = "6", IsCorrect = false }
    };
            var (addSuccess, addedQuestion) = await _questionService.AddQuestionAsync(question, choices);
            Assert.True(addSuccess);

            // Act
            var success = await _questionService.DeleteQuestionAsync(addedQuestion.QuestionId);

            // Assert
            Assert.True(success);

            // Verify in database
            var dbQuestion = await _context.Questions.FindAsync(addedQuestion.QuestionId);
            Assert.Null(dbQuestion);
        }

        [Fact]
        public async Task DeleteQuestionAsync_QuestionNotFound_ReturnsFalse()
        {
            // Act
            var success = await _questionService.DeleteQuestionAsync(999); // Non-existent question

            // Assert
            Assert.False(success);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}