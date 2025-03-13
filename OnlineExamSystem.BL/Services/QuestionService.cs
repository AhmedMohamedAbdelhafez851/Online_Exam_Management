using OnlineExamSystem.BL.UnitOfWork;
using OnlineExamSystem.Domains.Entities;
using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.BL.Abstraction;

namespace OnlineExamSystem.BL.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public QuestionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, Question? question)> AddQuestionAsync(Question question, List<Choice> choices)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                Console.WriteLine($"[AddQuestionAsync] Starting: Question={question.Title}, ExamId={question.ExamId}");

                var exam = await _unitOfWork.Repository<Exam>()
                    .GetAllIncludingAsync(e => e.Questions)
                    .Result
                    .FirstOrDefaultAsync(e => e.ExamId == question.ExamId);

                if (exam == null)
                {
                    Console.WriteLine($"[AddQuestionAsync] Error: Exam with ID {question.ExamId} not found.");
                    return (false, null);
                }
                Console.WriteLine($"[AddQuestionAsync] Found Exam: Title={exam.Title}, ExamId={exam.ExamId}");

                question.Exam = exam;
                Console.WriteLine($"[AddQuestionAsync] Set Question.Exam to ExamId={exam.ExamId}");

                await _unitOfWork.Repository<Question>().AddAsync(question);
                int questionSaveResult = await _unitOfWork.SaveChangesAsync();
                Console.WriteLine($"[AddQuestionAsync] Question saved with ID={question.QuestionId}, SaveChanges result={questionSaveResult} rows affected");

                foreach (var choice in choices)
                {
                    choice.QuestionId = question.QuestionId;
                    choice.Question = question;
                    await _unitOfWork.Repository<Choice>().AddAsync(choice);
                    Console.WriteLine($"[AddQuestionAsync] Added choice: Text={choice.Text}, IsCorrect={choice.IsCorrect}, ChoiceId={choice.ChoiceId}");
                }
                int choicesSaveResult = await _unitOfWork.SaveChangesAsync();
                Console.WriteLine($"[AddQuestionAsync] Choices saved, SaveChanges result={choicesSaveResult} rows affected");

                var correctChoice = choices.FirstOrDefault(c => c.IsCorrect);
                if (correctChoice == null)
                {
                    Console.WriteLine("[AddQuestionAsync] Error: No correct choice found in the choices list.");
                    return (false, question);
                }
                Console.WriteLine($"[AddQuestionAsync] Found correct choice: Text={correctChoice.Text}, ChoiceId={correctChoice.ChoiceId}");

                question.CorrectChoiceId = correctChoice.ChoiceId;
                question.CorrectChoice = correctChoice;
                await _unitOfWork.Repository<Question>().UpdateAsync(question);
                int updateSaveResult = await _unitOfWork.SaveChangesAsync();
                Console.WriteLine($"[AddQuestionAsync] Updated Question with CorrectChoiceId={question.CorrectChoiceId}, SaveChanges result={updateSaveResult} rows affected");

                if (exam.Questions == null)
                {
                    exam.Questions = new List<Question>();
                }
                if (!exam.Questions.Any(q => q.QuestionId == question.QuestionId))
                {
                    exam.Questions.Add(question);
                    await _unitOfWork.Repository<Exam>().UpdateAsync(exam);
                    int examSaveResult = await _unitOfWork.SaveChangesAsync();
                    Console.WriteLine($"[AddQuestionAsync] Added question to Exam {exam.ExamId} Questions list, SaveChanges result={examSaveResult} rows affected");
                }
                else
                {
                    Console.WriteLine($"[AddQuestionAsync] Warning: Question {question.QuestionId} already exists in Exam {exam.ExamId} Questions list.");
                }

                transaction.Commit();
                Console.WriteLine($"[AddQuestionAsync] Transaction committed for QuestionId={question.QuestionId}");
                Console.WriteLine($"[AddQuestionAsync] Completed successfully with QuestionId={question.QuestionId}");
                return (true, question);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"[AddQuestionAsync] Error: {ex.Message}\nStackTrace: {ex.StackTrace}\nInnerException: {ex.InnerException?.Message}");
                return (false, null);
            }
        }

              public async Task<(bool Success, Question? question)> EditQuestionAsync(Question question, List<Choice> choices)
        {
            try
            {
                Console.WriteLine($"[EditQuestionAsync] Starting: QuestionId={question.QuestionId}, Title={question.Title}");

                var existingQuestion = await _unitOfWork.Repository<Question>()
                    .GetAllIncludingAsync(q => q.Choices)
                    .Result
                    .FirstOrDefaultAsync(q => q.QuestionId == question.QuestionId);

                if (existingQuestion == null)
                {
                    Console.WriteLine($"[EditQuestionAsync] Error: Question with ID {question.QuestionId} not found.");
                    return (false, null);
                }

                existingQuestion.Title = question.Title;

                var existingChoices = existingQuestion.Choices.ToList();
                foreach (var existingChoice in existingChoices)
                {
                    var updatedChoice = choices.FirstOrDefault(c => c.ChoiceId == existingChoice.ChoiceId);
                    if (updatedChoice != null)
                    {
                        existingChoice.Text = updatedChoice.Text;
                        existingChoice.IsCorrect = updatedChoice.IsCorrect;
                        await _unitOfWork.Repository<Choice>().UpdateAsync(existingChoice);
                    }
                    else
                    {
                        await _unitOfWork.Repository<Choice>().DeleteAsync(existingChoice);
                    }
                }

                foreach (var choice in choices.Where(c => c.ChoiceId == 0))
                {
                    choice.QuestionId = question.QuestionId;
                    await _unitOfWork.Repository<Choice>().AddAsync(choice);
                }

                await _unitOfWork.SaveChangesAsync();

                var correctChoice = choices.FirstOrDefault(c => c.IsCorrect);
                if (correctChoice != null)
                {
                    existingQuestion.CorrectChoiceId = correctChoice.ChoiceId;
                    // Get the tracked choice from existingQuestion.Choices
                    var trackedCorrectChoice = existingQuestion.Choices.FirstOrDefault(c => c.ChoiceId == correctChoice.ChoiceId);
                    existingQuestion.CorrectChoice = trackedCorrectChoice!;
                }
                else
                {
                    existingQuestion.CorrectChoiceId = null;
                    existingQuestion.CorrectChoice = null!;
                }

                await _unitOfWork.Repository<Question>().UpdateAsync(existingQuestion);
                await _unitOfWork.SaveChangesAsync();

                Console.WriteLine($"[EditQuestionAsync] Completed successfully for QuestionId={existingQuestion.QuestionId}");
                return (true, existingQuestion);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EditQuestionAsync] Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return (false, null);
            }
        }

              public async Task<bool> DeleteQuestionAsync(int questionId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                Console.WriteLine($"[DeleteQuestionAsync] Starting: QuestionId={questionId}");

                var question = await _unitOfWork.Repository<Question>()
                    .GetAllIncludingAsync(q => q.Choices)
                    .Result
                    .FirstOrDefaultAsync(q => q.QuestionId == questionId);

                if (question == null)
                {
                    Console.WriteLine($"[DeleteQuestionAsync] Error: Question with ID {questionId} not found.");
                    return false;
                }

                // Step 1: Delete all related UserAnswers
                var userAnswers = await _unitOfWork.Repository<UserAnswer>()
                    .GetListAsync(ua => ua.QuestionId == questionId);

                foreach (var userAnswer in userAnswers)
                {
                    await _unitOfWork.Repository<UserAnswer>().DeleteAsync(userAnswer);
                }
                await _unitOfWork.SaveChangesAsync();
                Console.WriteLine($"[DeleteQuestionAsync] Deleted {userAnswers.Count} related user answers.");

                // Step 2: Clear CorrectChoiceId to avoid Restrict constraint
                question.CorrectChoiceId = null;
                await _unitOfWork.Repository<Question>().UpdateAsync(question);
                await _unitOfWork.SaveChangesAsync();
                Console.WriteLine($"[DeleteQuestionAsync] Cleared CorrectChoiceId for QuestionId={questionId}");

                // Step 3: Remove the question from the Exam's Questions list
                var exam = await _unitOfWork.Repository<Exam>()
                    .GetAllIncludingAsync(e => e.Questions)
                    .Result
                    .FirstOrDefaultAsync(e => e.ExamId == question.ExamId);

                if (exam != null && exam.Questions != null)
                {
                    var questionToRemove = exam.Questions.FirstOrDefault(q => q.QuestionId == questionId);
                    if (questionToRemove != null)
                    {
                        exam.Questions.Remove(questionToRemove);
                        await _unitOfWork.Repository<Exam>().UpdateAsync(exam);
                        await _unitOfWork.SaveChangesAsync();
                        Console.WriteLine($"[DeleteQuestionAsync] Removed question {questionId} from Exam {exam.ExamId}");
                    }
                    else
                    {
                        // If not in the list, delete explicitly
                        await _unitOfWork.Repository<Question>().DeleteAsync(question);
                        await _unitOfWork.SaveChangesAsync();
                        Console.WriteLine($"[DeleteQuestionAsync] Explicitly deleted question {questionId}");
                    }
                }
                else
                {
                    Console.WriteLine($"[DeleteQuestionAsync] Error: Exam with ID {question.ExamId} not found.");
                    return false;
                }

                transaction.Commit();
                Console.WriteLine($"[DeleteQuestionAsync] Transaction committed for QuestionId={questionId}");
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"[DeleteQuestionAsync] Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
                return false;
            }
        }

    }
}
