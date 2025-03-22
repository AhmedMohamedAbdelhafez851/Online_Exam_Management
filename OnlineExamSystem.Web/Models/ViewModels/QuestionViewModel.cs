using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamSystem.Web.Models.ViewModels
{
    public class ExactlyFourChoicesAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is List<ChoiceViewModel> choices)
            {
                if (choices.Count != 4)
                {
                    return new ValidationResult("Exactly 4 choices are required.");
                }
            }
            return ValidationResult.Success;
        }
    }
    public class AllChoicesHaveTextAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is List<ChoiceViewModel> choices)
            {
                foreach (var choice in choices)
                {
                    if (string.IsNullOrEmpty(choice.Text))
                    {
                        return new ValidationResult(
                            "All choices must have valid text.",
                            new[] { validationContext.MemberName }
                        );
                    }
                }
            }
            return ValidationResult.Success!;
        }
    }

    public class QuestionViewModel
    {
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Question text is required")]
        public string Title { get; set; }

        [Required]
        public int ExamId { get; set; }

        [NotMapped] // Prevents model binding and validation for this property during POST
        public string? ExamTitle { get; set; }

        [Range(0, 3, ErrorMessage = "You must select a correct answer")]
        public int CorrectChoiceIndex { get; set; }

        [Required(ErrorMessage = "Choices are required.")]
        [ExactlyFourChoices]
        [AllChoicesHaveText]
        public List<ChoiceViewModel> Choices { get; set; } = new();
    }
}