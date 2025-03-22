using Moq;
using OnlineExamSystem.BL.Services;
using OnlineExamSystem.BL.UnitOfWork;
using OnlineExamSystem.Domains.Entities;
using Xunit;

namespace OnlineExamSystem.Tests.ServicesTests
{
    public class ExamServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow = new();
        private readonly ExamService _service;

        public ExamServiceTests() => _service = new ExamService(_mockUow.Object);

        [Fact]
        public async Task CreateExam_ValidInput_ReturnsExam()
        {
            // Arrange
            var exam = new Exam { Title = "Math Final" };
            _mockUow.Setup(u => u.Repository<Exam>().AddAsync(exam))
                .ReturnsAsync(exam);

            // Act
            var result = await _service.CreateExamAsync(exam);

            // Assert
            _mockUow.Verify(u => u.SaveChangesAsync(), Times.Once);
            Assert.Equal("Math Final", result.Title);
        }
    }
}
