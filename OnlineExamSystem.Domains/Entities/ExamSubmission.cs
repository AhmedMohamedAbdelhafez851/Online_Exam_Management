namespace OnlineExamSystem.Domains.Entities
{
    public class ExamSubmission
    {
        public int SubmissionId { get; set; }
        public string UserId { get; set; } = "";
        public int ExamId { get; set; }
        public DateTime SubmissionDate { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions { get; set; }
        public double Score { get; set; } // Percentage
        public bool IsPassed { get; set; }

        public ApplicationUser User { get; set; } = null!;
        public Exam Exam { get; set; } = null!;
        public List<UserAnswer> Answers { get; set; } = new();
    }

    public class UserAnswer
    {
        public int UserAnswerId { get; set; }
        public int SubmissionId { get; set; }
        public int QuestionId { get; set; }
        public int SelectedChoiceId { get; set; }

        public ExamSubmission Submission { get; set; } = null!;
        public Question Question { get; set; } = null!;
        public Choice SelectedChoice { get; set; } = null!;
    }
}