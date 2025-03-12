using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.BL.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;
using OnlineExamSystem.BL.Abstraction;

namespace OnlineExamSystem.Web.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class QuestionController : Controller
    {
        private readonly IQuestionService _questionService;
        private readonly IUnitOfWork _unitOfWork;

        public QuestionController(IQuestionService questionService, IUnitOfWork unitOfWork)
        {
            _questionService = questionService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int? examId)
        {
            Console.WriteLine($"Index method called with examId: {examId}");

            if (!examId.HasValue)
            {
                return RedirectToAction("Index", "Exam"); // Redirect if no examId is provided
            }

            // Fetch the exam title
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(examId.Value);
            if (exam == null)
            {
                return NotFound(); // Exam not found
            }

            // Fetch questions for the specific exam
            var questions = await _unitOfWork.Repository<Question>()
                .GetAllIncludingAsync(q => q.Choices)
                .Result
                .Where(q => q.ExamId == examId.Value)
                .ToListAsync();

            // Pass the exam title to the view
            ViewBag.ExamTitle = exam.Title;
            ViewBag.ExamId = examId;
            return View(questions);
        }

        public IActionResult Add(int? examId)
        {
            Console.WriteLine($"Add GET method called with examId: {examId}");

            if (!examId.HasValue)
            {
                return RedirectToAction("Index", "Exam"); // Redirect if no examId is provided
            }

            // Fetch the exam title
            var exam = _unitOfWork.Repository<Exam>().GetByIdAsync(examId.Value).Result;
            if (exam == null)
            {
                return NotFound(); // Exam not found
            }

            var model = new QuestionViewModel
            {
                CorrectChoiceIndex = -1,
                ExamId = examId.Value,
                ExamTitle = exam.Title // Set the exam title
            };

            for (int i = 0; i < 4; i++)
            {
                model.Choices.Add(new ChoiceViewModel());
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(QuestionViewModel model)
        {
            Console.WriteLine($"Add POST method called with model.ExamId: {model.ExamId}");

            if (ModelState.IsValid)
            {
                if (model.Choices.Count != 4 || model.CorrectChoiceIndex < 0 || model.CorrectChoiceIndex >= 4)
                {
                    ModelState.AddModelError("", "Exactly 4 choices are required, and one must be marked as correct.");
                    return View(model);
                }

                if (model.Choices.Any(c => string.IsNullOrWhiteSpace(c.Text)))
                {
                    ModelState.AddModelError("", "All choices must have text.");
                    return View(model);
                }

                for (int i = 0; i < model.Choices.Count; i++)
                {
                    model.Choices[i].IsCorrect = (i == model.CorrectChoiceIndex);
                }

                var question = new Question
                {
                    Title = model.Title,
                    ExamId = model.ExamId
                };

                var choices = model.Choices.Select(c => new Choice
                {
                    Text = c.Text,
                    IsCorrect = c.IsCorrect
                }).ToList();

                var result = await _questionService.AddQuestionAsync(question, choices);

                if (result.Success && result.question != null)
                {
                    return RedirectToAction("Index", new { examId = model.ExamId });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to add question. Please try again.");
                }
            }

            return View(model);
        }

        // Add Edit GET action
        public async Task<IActionResult> Edit(int id, int examId)
        {
            Console.WriteLine($"Edit GET method called with id: {id}, examId: {examId}");

            var question = await _unitOfWork.Repository<Question>()
                .GetAllIncludingAsync(q => q.Choices)
                .Result
                .FirstOrDefaultAsync(q => q.QuestionId == id && q.ExamId == examId);

            if (question == null)
            {
                return NotFound();
            }

            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(examId);
            if (exam == null)
            {
                return NotFound();
            }

            var model = new QuestionViewModel
            {
                QuestionId = question.QuestionId,
                Title = question.Title,
                ExamId = question.ExamId,
                ExamTitle = exam.Title,
                Choices = question.Choices.Select(c => new ChoiceViewModel
                {
                    ChoiceId = c.ChoiceId,
                    Text = c.Text,
                    IsCorrect = c.IsCorrect
                }).ToList()
            };

            // Set the CorrectChoiceIndex based on the IsCorrect property
            model.CorrectChoiceIndex = model.Choices.FindIndex(c => c.IsCorrect);
            if (model.CorrectChoiceIndex == -1)
            {
                model.CorrectChoiceIndex = 0; // Default to first choice if none is correct
            }

            return View(model);
        }

        // Add Edit POST action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(QuestionViewModel model)
        {
            Console.WriteLine($"Edit POST method called with QuestionId: {model.QuestionId}, ExamId: {model.ExamId}");

            if (ModelState.IsValid)
            {
                if (model.Choices.Count != 4 || model.CorrectChoiceIndex < 0 || model.CorrectChoiceIndex >= 4)
                {
                    ModelState.AddModelError("", "Exactly 4 choices are required, and one must be marked as correct.");
                    return View(model);
                }

                if (model.Choices.Any(c => string.IsNullOrWhiteSpace(c.Text)))
                {
                    ModelState.AddModelError("", "All choices must have text.");
                    return View(model);
                }

                var question = await _unitOfWork.Repository<Question>()
                    .GetAllIncludingAsync(q => q.Choices)
                    .Result
                    .FirstOrDefaultAsync(q => q.QuestionId == model.QuestionId);

                if (question == null)
                {
                    return NotFound();
                }

                // Update question title
                question.Title = model.Title;

                // Update choices
                var choices = new List<Choice>();
                for (int i = 0; i < model.Choices.Count; i++)
                {
                    var choice = new Choice
                    {
                        ChoiceId = model.Choices[i].ChoiceId,
                        Text = model.Choices[i].Text,
                        IsCorrect = (i == model.CorrectChoiceIndex),
                        QuestionId = question.QuestionId
                    };
                    choices.Add(choice);
                }

                var result = await _questionService.EditQuestionAsync(question, choices);

                if (result.Success && result.question != null)
                {
                    return RedirectToAction("Index", new { examId = model.ExamId });
                }
                else
                {
                    ModelState.AddModelError("", "Failed to update question. Please try again.");
                }
            }

            // If model state is invalid, fetch the exam title again for the view
            var exam = await _unitOfWork.Repository<Exam>().GetByIdAsync(model.ExamId);
            model.ExamTitle = exam?.Title;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int examId)
        {
            Console.WriteLine($"Delete POST method called with id: {id}, examId: {examId}");

            var result = await _questionService.DeleteQuestionAsync(id);
            if (result)
            {
                return Json(new { success = true, message = "Question deleted successfully" });
            }
            return Json(new { success = false, message = "Failed to delete question" });
        }
    }
}