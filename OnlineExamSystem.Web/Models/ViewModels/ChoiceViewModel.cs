using System.ComponentModel.DataAnnotations;

namespace OnlineExamSystem.Web.Models.ViewModels
{
    public class ChoiceViewModel
    {
        public int ChoiceId { get; set; }

        [Required(ErrorMessage = "Choice text is required")]
        public string Text { get; set; }

        public bool IsCorrect { get; set; }
    }
}
