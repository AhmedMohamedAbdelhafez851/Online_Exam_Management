using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineExamSystem.Web.Models.ViewModels
{
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
        public List<ChoiceViewModel> Choices { get; set; } = new();
    }

}