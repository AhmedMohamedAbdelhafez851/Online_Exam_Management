using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineExamSystem.Domains.Entities
{
    public class Question
    {
        public int QuestionId { get; set; }
        public int ExamId { get; set; }
        public string Title { get; set; } = "";
        public int? CorrectChoiceId { get; set; } // Foreign key to the correct Choice
        public Choice CorrectChoice { get; set; } // Navigation property for the correct Choice
        public Exam Exam { get; set; } = new();
        public List<Choice> Choices { get; set; } = new();
    }
}
