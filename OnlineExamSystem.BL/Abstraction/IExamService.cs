using OnlineExamSystem.Domains.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineExamSystem.BL.Abstraction
{
    public interface IExamService
    {
        Task<IQueryable<Exam>> GetAllExamsAsync();
        Task<Exam> CreateExamAsync(Exam exam);
        Task<Exam> EditExamAsync(Exam exam);
        Task DeleteExamAsync(int examId);
    }
}