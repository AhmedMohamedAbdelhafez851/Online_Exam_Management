using OnlineExamSystem.Domains.Entities;

namespace OnlineExamSystem.BL.Abstraction
{
    public interface IExamSubmissionService
    {
        Task<List<ExamSubmission>> GetUserSubmissionsAsync(string userId);
        Task<ExamSubmission> SubmitExamAsync(string userId, int examId, Dictionary<int, int> answers);
    }
}