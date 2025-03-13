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
     => await _unitOfWork.Repository<Exam>().GetAllIncludingAsync(e => e.Questions);

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
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the exam with all nested dependencies
                var exam = await _unitOfWork.Repository<Exam>()
                    .GetAllWithNestedIncludesAsync(query => query
                        .Include(e => e.Questions)
                        .ThenInclude(q => q.Choices)
                        .ThenInclude(c => c.UserAnswers)) // Now valid
                    .Result
                    .FirstOrDefaultAsync(e => e.ExamId == examId);

                if (exam == null) return;

                // Delete all UserAnswers linked to the exam's choices
                foreach (var question in exam.Questions)
                {
                    foreach (var choice in question.Choices)
                    {
                        if (choice.UserAnswers.Any())
                        {
                            foreach (var userAnswer in choice.UserAnswers.ToList())
                            {
                                await _unitOfWork.Repository<UserAnswer>().DeleteAsync(userAnswer);
                            }
                        }
                    }
                }

                // Save changes after deleting UserAnswers
                await _unitOfWork.SaveChangesAsync();

                // Delete the exam (this will cascade delete Questions and Choices)
                await _unitOfWork.Repository<Exam>().DeleteAsync(exam);
                await _unitOfWork.SaveChangesAsync();

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
