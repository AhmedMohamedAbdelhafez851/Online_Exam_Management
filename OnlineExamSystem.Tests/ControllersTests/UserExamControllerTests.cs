using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using OnlineExamSystem.BL.Abstraction;
using OnlineExamSystem.BL.Repositories;
using OnlineExamSystem.BL.UnitOfWork;
using OnlineExamSystem.Domains;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Web.Controllers;
using System.Security.Claims;
using Xunit;

namespace OnlineExamSystem.Tests.ControllersTests
{
    public class UserExamControllerTests : IDisposable
    {
        private readonly Mock<IExamService> _mockExamService = new();
        private readonly Mock<IExamSubmissionService> _mockSubmissionService = new();
        private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly UserExamController _controller;
        private readonly DbContextOptions<ApplicationDbContext> _dbOptions;

        public UserExamControllerTests()
        {
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDB")
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            // Reset the database before each test
            using var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var store = new Mock<IUserStore<ApplicationUser>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[] { new Claim(ClaimTypes.NameIdentifier, "test-user-id") },
                "TestAuthentication"));

            _controller = new UserExamController(
                _mockExamService.Object,
                _mockSubmissionService.Object,
                _mockUnitOfWork.Object,
                _mockUserManager.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };
        }

        [Fact]
        public async Task Index_ReturnsViewWithExamsAndSubmissions()
        {
            // Arrange - Use real DbContext for async operations
            await using var context = new ApplicationDbContext(_dbOptions);
            await context.Exams.AddRangeAsync(
                new Exam { ExamId = 1 },
                new Exam { ExamId = 2 }
            );
            await context.SaveChangesAsync();

            var submissions = new List<ExamSubmission> { new ExamSubmission { ExamId = 1 } };

            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("test-user-id");
            _mockExamService.Setup(s => s.GetAllExamsAsync())
                .ReturnsAsync(context.Exams.AsQueryable());
            _mockSubmissionService.Setup(s => s.GetUserSubmissionsAsync("test-user-id"))
                .ReturnsAsync(submissions);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult.ViewData["Submissions"]);
            Assert.Equal(2, ((List<Exam>)viewResult.Model!).Count);
        }

        [Fact]
        public async Task TakeExam_ValidId_ReturnsView()
        {
            // Arrange - Use real DbContext
            await using var context = new ApplicationDbContext(_dbOptions);
            var exam = new Exam
            {
                ExamId = 1,
                Questions = new List<Question>
                {
                    new Question { Choices = new List<Choice>() }
                }
            };
            await context.Exams.AddAsync(exam);
            await context.SaveChangesAsync();

            var mockRepo = new Mock<IRepository<Exam>>();
            mockRepo.Setup(r => r.GetAllWithNestedIncludesAsync(It.IsAny<Func<IQueryable<Exam>, IQueryable<Exam>>>()))
                .ReturnsAsync(context.Exams.AsQueryable());

            _mockUnitOfWork.Setup(u => u.Repository<Exam>()).Returns(mockRepo.Object);

            // Act
            var result = await _controller.TakeExam(1);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task SubmitExam_ValidData_ReturnsSuccessJson()
        {
            // Arrange
            var submission = new ExamSubmission { SubmissionId = 1 };
            _mockUserManager.Setup(m => m.GetUserId(It.IsAny<ClaimsPrincipal>()))
                .Returns("test-user-id");
            _mockSubmissionService.Setup(s => s.SubmitExamAsync("test-user-id", 1, It.IsAny<Dictionary<int, int>>()))
                .ReturnsAsync(submission);

            // Act
            var result = await _controller.SubmitExam(1, new Dictionary<int, int>());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);

            // Safe null handling
            Assert.NotNull(jsonResult.Value);
            var valueType = jsonResult.Value.GetType();

            // Check success property
            var successProperty = valueType.GetProperty("success");
            Assert.NotNull(successProperty);
            var success = (bool)successProperty.GetValue(jsonResult.Value)!;

            // Check submissionId property
            var submissionIdProperty = valueType.GetProperty("submissionId");
            Assert.NotNull(submissionIdProperty);
            var submissionId = (int)submissionIdProperty.GetValue(jsonResult.Value)!;

            Assert.True(success);
            Assert.Equal(1, submissionId);
        }

        public void Dispose()
        {
            using var context = new ApplicationDbContext(_dbOptions);
            context.Database.EnsureDeleted();
        }
    }
}