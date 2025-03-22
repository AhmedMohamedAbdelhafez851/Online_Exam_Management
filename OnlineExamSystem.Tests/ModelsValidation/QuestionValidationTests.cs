using Microsoft.AspNetCore.Mvc.ModelBinding;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Web.Models.ViewModels;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace OnlineExamSystem.Tests.ModelsValidation
{
    public class QuestionValidationTests
    {
        [Fact]
        public void Question_TitleRequired_ValidationFails()
        {
            // Arrange
            var question = new Question { Title = "" };

            // Act
            var results = ValidateModel(question);

            // Assert
            Assert.Contains(results, r =>
                r.ErrorMessage!.Contains("required", StringComparison.OrdinalIgnoreCase) &&
                r.MemberNames.Contains(nameof(Question.Title)));
        }

        [Fact]
        public void Question_TitleLengthValidation_WorksCorrectly()
        {
            // Arrange
            var shortTitleQuestion = new Question { Title = "A" };
            var longTitleQuestion = new Question { Title = new string('A', 501) };
            var validQuestion = new Question { Title = "Valid Question (3-500 chars)" };

            // Act
            var shortResults = ValidateModel(shortTitleQuestion);
            var longResults = ValidateModel(longTitleQuestion);
            var validResults = ValidateModel(validQuestion);

            // Assert
            // Check short title
            Assert.Contains(shortResults, r =>
                r.ErrorMessage!.Contains("between 3-500 characters") &&
                r.MemberNames.Contains(nameof(Question.Title)));

            // Check long title
            Assert.Contains(longResults, r =>
                r.ErrorMessage!.Contains("between 3-500 characters") &&
                r.MemberNames.Contains(nameof(Question.Title)));

            // Check valid title
            Assert.Empty(validResults);
        }

        [Fact]
        public void QuestionViewModel_ValidatesCorrectChoiceSelection()
        {
            // Arrange
            var model = new QuestionViewModel
            {
                Title = "Valid Title",
                CorrectChoiceIndex = -1,
                Choices = Enumerable.Repeat(new ChoiceViewModel(), 4).ToList()
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r => r.ErrorMessage!.Contains("select a correct answer"));
        }

        [Fact]
        public void QuestionViewModel_ValidModel_PassesValidation()
        {
            // Arrange
            var model = new QuestionViewModel
            {
                Title = "What is 2+2?",
                CorrectChoiceIndex = 1,
                Choices = new List<ChoiceViewModel>
                {
                    new() { Text = "3" },
                    new() { Text = "4" },
                    new() { Text = "5" },
                    new() { Text = "6" }
                }
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Empty(results);
        }

        // Updated Tests
        [Fact]
        public void QuestionViewModel_RequiresFourChoices()
        {
            // Arrange
            var model = new QuestionViewModel
            {
                Title = "Valid Title",
                CorrectChoiceIndex = 0,
                Choices = new List<ChoiceViewModel> // Only 3 choices
        {
            new() { Text = "A" },
            new() { Text = "B" },
            new() { Text = "C" }
        }
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r =>
                r.ErrorMessage!.Contains("Exactly 4 choices are required") &&
                r.MemberNames.Contains(nameof(QuestionViewModel.Choices)));
        }

        [Fact]
        public void QuestionViewModel_ValidatesChoiceTextPresence()
        {
            // Arrange
            var model = new QuestionViewModel
            {
                Title = "Valid Title",
                CorrectChoiceIndex = 0,
                Choices = new List<ChoiceViewModel>
        {
            new() { Text = "A" },
            new() { Text = "" },  // Invalid
            new() { Text = "C" },
            new() { Text = "D" }
        }
            };

            // Act
            var results = ValidateModel(model);

            // Assert
            Assert.Contains(results, r =>
                r.ErrorMessage!.Contains("All choices must have valid text") &&
                r.MemberNames.Contains(nameof(QuestionViewModel.Choices)));
        }


        private static List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);

            // Validate main model properties
            Validator.TryValidateObject(model, validationContext, validationResults, true);

            // Validate nested choices if present
            if (model is QuestionViewModel question)
            {
                foreach (var choice in question.Choices)
                {
                    var choiceContext = new ValidationContext(choice, null, null);
                    var choiceResults = new List<ValidationResult>();
                    Validator.TryValidateObject(choice, choiceContext, choiceResults, true);

                    // Map choice errors to parent Choices property
                    validationResults.AddRange(choiceResults.Select(r =>
                        new ValidationResult(r.ErrorMessage, new[] { nameof(QuestionViewModel.Choices) })));
                }
            }

            return validationResults;
        }
    }
}