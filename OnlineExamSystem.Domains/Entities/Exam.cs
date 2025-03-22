using System.ComponentModel.DataAnnotations;

namespace OnlineExamSystem.Domains.Entities
{
    public class Exam
    {
        public int ExamId { get; set; }

        [Required(ErrorMessage = "Title is required")]  // ← THIS IS CRUCIAL
        [StringLength(100, MinimumLength = 3)]
        public string Title { get; set; } = "";

        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public List<Question> Questions { get; set; } = new();
    }
}
