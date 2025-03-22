using OnlineExamSystem.Domains.Entities;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace OnlineExamSystem.Tests.ModelsValidation
{
    public class ExamValidationTests
    {
        [Fact]
        public void Exam_TitleRequired_ValidationFails()
        {
            // Arrange
            var exam = new Exam { Title = "" };  // Empty title
            var context = new ValidationContext(exam);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(exam, context, results, true);

            // Assert
            Assert.False(isValid, "Validation should fail for empty title");

            // Check for required error
            var hasRequiredError = results.Any(r =>
                r.MemberNames.Contains(nameof(Exam.Title)) &&
                r.ErrorMessage!.Contains("required", StringComparison.OrdinalIgnoreCase));

            Assert.True(hasRequiredError, "Should have 'required' error for Title");
        }
    }
}