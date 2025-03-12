using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.BL.Abstraction;
using OnlineExamSystem.BL.UnitOfWork;
using OnlineExamSystem.Domains.Entities;

namespace OnlineExamSystem.BL.Services
{
    public class ExamSubmissionService : IExamSubmissionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamSubmissionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ExamSubmission>> GetUserSubmissionsAsync(string userId)
        {
            return await _unitOfWork.Repository<ExamSubmission>()
                .GetListAsync(es => es.UserId == userId);
        }

        public async Task<ExamSubmission> SubmitExamAsync(string userId, int examId, Dictionary<int, int> answers)
        {
            var exam = await _unitOfWork.Repository<Exam>()
                .GetAllWithNestedIncludesAsync(q => q
                    .Include(e => e.Questions)
                    .ThenInclude(q => q.Choices))
                .Result
                .FirstOrDefaultAsync(e => e.ExamId == examId);

            if (exam == null) throw new Exception("Exam not found.");

            var submission = new ExamSubmission
            {
                UserId = userId, // Use the actual Id from AspNetUsers
                ExamId = examId,
                SubmissionDate = DateTime.UtcNow,
                TotalQuestions = exam.Questions.Count,
                Answers = new List<UserAnswer>()
            };

            int correctAnswers = 0;
            foreach (var answer in answers)
            {
                var question = exam.Questions.FirstOrDefault(q => q.QuestionId == answer.Key);
                if (question != null)
                {
                    var userAnswer = new UserAnswer
                    {
                        QuestionId = question.QuestionId,
                        SelectedChoiceId = answer.Value
                    };
                    submission.Answers.Add(userAnswer);

                    if (question.CorrectChoiceId == answer.Value)
                    {
                        correctAnswers++;
                    }
                }
            }

            submission.CorrectAnswers = correctAnswers;
            submission.Score = (double)correctAnswers / submission.TotalQuestions * 100;
            submission.IsPassed = submission.Score >= 50; // Example passing threshold

            await _unitOfWork.Repository<ExamSubmission>().AddAsync(submission);
            await _unitOfWork.SaveChangesAsync();

            return submission;
        }
    }
}