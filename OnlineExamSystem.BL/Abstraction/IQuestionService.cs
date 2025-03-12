using OnlineExamSystem.Domains.Entities;
using System.Threading.Tasks;

namespace OnlineExamSystem.BL.Abstraction
{
    public interface IQuestionService
    {
        Task<(bool Success, Question? question)> AddQuestionAsync(Question question, List<Choice> choices);
        Task<(bool Success, Question? question)> EditQuestionAsync(Question question, List<Choice> choices);
        Task<bool> DeleteQuestionAsync(int questionId);
    }
}