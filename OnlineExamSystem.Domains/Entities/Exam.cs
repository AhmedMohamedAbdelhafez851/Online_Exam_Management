using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineExamSystem.Domains.Entities
{
    public class Exam
    {
        public int ExamId { get; set; }
        public string Title { get; set; } = ""; 
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public List<Question> Questions { get; set; } = new();
    }
}

