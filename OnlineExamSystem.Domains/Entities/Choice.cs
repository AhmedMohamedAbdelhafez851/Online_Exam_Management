using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineExamSystem.Domains.Entities
{
    public class Choice
    {
        public int ChoiceId { get; set; }
        public int QuestionId { get; set; }
        public string Text { get; set; } = "";

        public bool IsCorrect { get; set; }
        public virtual Question Question { get; set; } = new();
    }
}
