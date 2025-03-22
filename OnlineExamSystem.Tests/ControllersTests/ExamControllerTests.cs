using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineExamSystem.BL.Abstraction;
using OnlineExamSystem.Domains;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Web.Controllers;
using OnlineExamSystem.Web.Models.ViewModels;
using System.Security.Claims;
using Xunit;

namespace OnlineExamSystem.Tests.ControllersTests
{
    public class ExamControllerTests
    {
        private readonly Mock<IExamService> _mockService = new();
        private readonly ExamController _controller;

        public ExamControllerTests() => _controller = new ExamController(_mockService.Object);

        [Fact]
        public async Task Index_ReturnsViewWithExams()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            await using var context = new ApplicationDbContext(options);
            await context.Exams.AddAsync(new Exam { Title = "Test" });
            await context.SaveChangesAsync();

            var examsQueryable = context.Exams.AsQueryable();

            _mockService.Setup(s => s.GetAllExamsAsync())
                .ReturnsAsync(examsQueryable);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Exam>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("Test", model[0].Title);
        }

        [Fact]
        public void Create_GET_ReturnsView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_POST_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var model = new ExamViewModel { Title = "Valid Exam" };

            // Mock User.Identity
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[] { new Claim(ClaimTypes.Name, "testuser") },
                "TestAuthentication"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            // Setup mock service
            _mockService.Setup(s => s.CreateExamAsync(It.IsAny<Exam>()))
                .ReturnsAsync(new Exam { Title = model.Title });

            // Act
            var result = await _controller.Create(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _mockService.Verify(s => s.CreateExamAsync(It.Is<Exam>(e =>
                e.Title == model.Title &&
                e.CreatedBy == "testuser")), Times.Once);
        }

        [Fact]
        public async Task Create_POST_InvalidModel_ReturnsView()
        {
            // Arrange
            _controller.ModelState.AddModelError("Title", "Required");
            var model = new ExamViewModel();

            // Act
            var result = await _controller.Create(model);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Edit_GET_ValidId_ReturnsView()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_EditView")
                .Options;

            await using var context = new ApplicationDbContext(options);
            var exam = new Exam { ExamId = 1, Title = "Test" };
            await context.Exams.AddAsync(exam);
            await context.SaveChangesAsync();

            var examsQueryable = context.Exams.AsQueryable();

            _mockService.Setup(s => s.GetAllExamsAsync())
                .ReturnsAsync(examsQueryable);

            // Act
            var result = await _controller.Edit(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ExamViewModel>(viewResult.Model);
            Assert.Equal(exam.Title, model.Title);
        }
        [Fact]
        public async Task Edit_GET_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_InvalidId")
                .Options;

            await using var context = new ApplicationDbContext(options);

            // Add a valid exam to ensure the invalid ID check works
            await context.Exams.AddAsync(new Exam { ExamId = 1, Title = "Valid Exam" });
            await context.SaveChangesAsync();

            var examsQueryable = context.Exams.AsQueryable();

            _mockService.Setup(s => s.GetAllExamsAsync())
                .ReturnsAsync(examsQueryable);

            // Act
            var result = await _controller.Edit(999); // Non-existent ID

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_POST_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_EditPost")
                .Options;

            await using var context = new ApplicationDbContext(options);
            var exam = new Exam { ExamId = 1, Title = "Original" };
            await context.Exams.AddAsync(exam);
            await context.SaveChangesAsync();

            var examsQueryable = context.Exams.AsQueryable();
            var model = new ExamViewModel { Title = "Updated" };

            _mockService.Setup(s => s.GetAllExamsAsync())
                .ReturnsAsync(examsQueryable);

            // Act
            var result = await _controller.Edit(1, model);

            // Assert
            Assert.Equal("Updated", exam.Title);
            _mockService.Verify(s => s.EditExamAsync(It.Is<Exam>(e =>
                e.ExamId == 1 &&
                e.Title == "Updated")), Times.Once);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Delete_ValidId_RedirectsToIndex()
        {
            // Act
            var result = await _controller.Delete(1);

            // Assert
            _mockService.Verify(s => s.DeleteExamAsync(1), Times.Once);
            Assert.IsType<RedirectToActionResult>(result);
        }

        [Fact]
        public async Task Delete_ThrowsError_SetsTempData()
        {
            // Arrange
            // Mock TempData
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

            _controller.TempData = tempData;

            _mockService.Setup(s => s.DeleteExamAsync(1))
                .ThrowsAsync(new Exception("Test error"));

            // Act
            var result = await _controller.Delete(1);

            // Assert
            Assert.True(_controller.TempData.ContainsKey("ErrorMessage"));
            Assert.Equal("Failed to delete exam. It may have existing user data.",
                _controller.TempData["ErrorMessage"]);
            Assert.IsType<RedirectToActionResult>(result);
        }
    }
}