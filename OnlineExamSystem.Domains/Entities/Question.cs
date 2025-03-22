using System.ComponentModel.DataAnnotations;

namespace OnlineExamSystem.Domains.Entities
{
    public class Question
    {
        public int QuestionId { get; set; }

        [Required(ErrorMessage = "Question text is required")]
        [StringLength(500, MinimumLength = 3, ErrorMessage = "Question must be between {2}-{1} characters")]
        public string Title { get; set; } = "";

        public int ExamId { get; set; }
        public int? CorrectChoiceId { get; set; }
        public Choice CorrectChoice { get; set; }
        public Exam Exam { get; set; } 
        public List<Choice> Choices { get; set; } = new();
    }
}
