using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineExamSystem.BL.Abstraction;
using OnlineExamSystem.BL.Repositories;
using OnlineExamSystem.BL.UnitOfWork;
using OnlineExamSystem.Domains;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Web.Controllers;
using OnlineExamSystem.Web.Models.ViewModels;
using System.Linq.Expressions;
using System.Security.Claims;
using Xunit;

namespace OnlineExamSystem.Tests.ControllersTests
{
    public class QuestionControllerTests : IDisposable
    {
        private readonly Mock<IQuestionService> _mockQuestionService = new();
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly QuestionController _controller;
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly ApplicationDbContext _context;

        public QuestionControllerTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_options);

            // Mock TempData
            var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());

            _controller = new QuestionController(_mockQuestionService.Object, _mockUnitOfWork.Object)
            {
                TempData = tempData
            };
        }

        [Fact]
        public async Task Index_InvalidExamId_RedirectsToExamIndex()
        {
            // Act
            var result = await _controller.Index(null);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Exam", redirectResult.ControllerName);
        }

        [Fact]
        public async Task Index_ValidExamId_ReturnsViewWithQuestions()
        {
            // Arrange - Create fresh context with unique database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB_" + Guid.NewGuid())
                .Options;

            await using var context = new ApplicationDbContext(options);

            // Create and save test data
            var exam = new Exam { ExamId = 1, Title = "Test Exam" };
            var questions = new List<Question>
    {
        new Question { ExamId = 1, Title = "Q1" },
        new Question { ExamId = 1, Title = "Q2" }
    };

            await context.Exams.AddAsync(exam);
            await context.Questions.AddRangeAsync(questions);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Configure mocks with detached entities
            _mockUnitOfWork.Setup(u => u.Repository<Exam>().GetByIdAsync(1))
                .ReturnsAsync(await context.Exams.AsNoTracking().FirstAsync());

            _mockUnitOfWork.Setup(u => u.Repository<Question>()
                .GetAllIncludingAsync(It.IsAny<Expression<Func<Question, object>>[]>()))
                .ReturnsAsync(context.Questions.AsNoTracking().Include(q => q.Choices));

            // Act
            var result = await _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Question>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("Test Exam", viewResult.ViewData["ExamTitle"]);
        }
        [Fact]
        public void Add_GET_ReturnsViewWithEmptyQuestion()
        {
            // Arrange
            var exam = new Exam { ExamId = 1, Title = "Test Exam" };
            _mockUnitOfWork.Setup(u => u.Repository<Exam>().GetByIdAsync(1))
                .ReturnsAsync(exam);

            // Act
            var result = _controller.Add(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<QuestionViewModel>(viewResult.Model);
            Assert.Equal(4, model.Choices.Count);
            Assert.Equal("Test Exam", model.ExamTitle);
        }

        [Fact]
        public async Task Add_POST_InvalidModel_ReturnsViewWithErrors()
        {
            // Arrange
            var model = new QuestionViewModel
            {
                ExamId = 1,
                Choices = { new ChoiceViewModel(), new ChoiceViewModel() } // Only 2 choices
            };

            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.Add(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.Equal(2, model.Choices.Count);
        }

        [Fact]
        public async Task Edit_GET_InvalidId_ReturnsNotFound()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var mockRepo = new Mock<IRepository<Question>>();

            // Return empty queryable from real context
            mockRepo.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<Question, object>>[]>()))
                .ReturnsAsync(context.Questions.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Repository<Question>()).Returns(mockRepo.Object);

            // Act
            var result = await _controller.Edit(999, 1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        [Fact]
        public async Task Edit_POST_ValidModel_RedirectsToIndex()
        {
            // Arrange - Create fresh isolated context
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "EditTest_" + Guid.NewGuid())
                .Options;

            await using var context = new ApplicationDbContext(options);

            // Create clean test data
            var exam = new Exam { ExamId = 1, Title = "Test Exam" };
            var question = new Question
            {
                QuestionId = 1,
                ExamId = 1,
                Title = "Original",
                Choices = new List<Choice>
        {
            new Choice { ChoiceId = 1, Text = "A" },
            new Choice { ChoiceId = 2, Text = "B" },
            new Choice { ChoiceId = 3, Text = "C" },
            new Choice { ChoiceId = 4, Text = "D" }
        }
            };

            await context.Exams.AddAsync(exam);
            await context.Questions.AddAsync(question);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            // Configure mocks with detached entities
            var mockRepo = new Mock<IRepository<Question>>();
            mockRepo.Setup(r => r.GetAllIncludingAsync(It.IsAny<Expression<Func<Question, object>>[]>()))
                .ReturnsAsync(context.Questions.AsNoTracking().Include(q => q.Choices));

            _mockUnitOfWork.Setup(u => u.Repository<Question>()).Returns(mockRepo.Object);
            _mockUnitOfWork.Setup(u => u.Repository<Exam>().GetByIdAsync(1))
                .ReturnsAsync(await context.Exams.AsNoTracking().FirstAsync());

            // Mock service with proper tracking handling
            _mockQuestionService.Setup(s => s.EditQuestionAsync(It.IsAny<Question>(), It.IsAny<List<Choice>>()))
                .ReturnsAsync((Question q, List<Choice> c) => (true, q));

            // Create model with valid data
            var model = new QuestionViewModel
            {
                QuestionId = 1,
                ExamId = 1,
                Title = "Updated Question",
                CorrectChoiceIndex = 0,
                Choices = question.Choices.Select((c, i) => new ChoiceViewModel
                {
                    ChoiceId = c.ChoiceId,
                    Text = c.Text,
                    IsCorrect = i == 0
                }).ToList()
            };

            // Act
            var result = await _controller.Edit(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal(1, redirectResult.RouteValues!["examId"]);
        }
        [Fact]
        public async Task Delete_ValidId_ReturnsSuccessJson()
        {
            // Arrange
            _mockQuestionService.Setup(s => s.DeleteQuestionAsync(1))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.Delete(1, 1);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var success = jsonResult.Value!.GetType().GetProperty("success")?.GetValue(jsonResult.Value);
            var message = jsonResult.Value.GetType().GetProperty("message")?.GetValue(jsonResult.Value);

            Assert.True((bool)success!);
            Assert.Equal("Question deleted successfully", message);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}