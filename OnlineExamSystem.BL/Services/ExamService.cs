using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.BL.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.BL.Abstraction;

namespace OnlineExamSystem.BL.Services
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ExamService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public async Task<IQueryable<Exam>> GetAllExamsAsync()
            => await _unitOfWork.Repository<Exam>().GetAllIncludingAsync();

        public async Task<Exam> CreateExamAsync(Exam exam)
        {
            await _unitOfWork.Repository<Exam>().AddAsync(exam);
            await _unitOfWork.SaveChangesAsync();
            return exam;
        }

        public async Task<Exam> EditExamAsync(Exam exam)
        {
            await _unitOfWork.Repository<Exam>().UpdateAsync(exam);
            await _unitOfWork.SaveChangesAsync();
            return exam;
        }

        public async Task DeleteExamAsync(int examId)
        {
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(examId);
            if (exam != null)
            {
                await _unitOfWork.Repository<Exam>().DeleteAsync(exam);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}