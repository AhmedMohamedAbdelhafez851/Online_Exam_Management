using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.BL.Abstraction;
using OnlineExamSystem.BL.UnitOfWork;
using OnlineExamSystem.Domains.Entities;

namespace OnlineExamSystem.Web.Controllers
{
    [Authorize] // Ensure authentication is required
    public class UserExamController : Controller
    {
        private readonly IExamService _examService;
        private readonly IExamSubmissionService _submissionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserExamController(
            IExamService examService,
            IExamSubmissionService submissionService,
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _examService = examService;
            _submissionService = submissionService;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User) ?? "test-user"; // Use UserManager to get Id, with fallback
            var exams = await _examService.GetAllExamsAsync();
            var submissions = await _submissionService.GetUserSubmissionsAsync(userId);
            ViewBag.Submissions = submissions.ToDictionary(s => s.ExamId, s => s);
            return View(await exams.ToListAsync());
        }

        public async Task<IActionResult> TakeExam(int id)
        {
            var exam = await _unitOfWork.Repository<Exam>()
                .GetAllWithNestedIncludesAsync(q => q
                    .Include(e => e.Questions)
                    .ThenInclude(q => q.Choices))
                .Result
                .FirstOrDefaultAsync(e => e.ExamId == id);

            if (exam == null) return NotFound();

            return View(exam);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitExam(int examId, Dictionary<int, int> answers)
        {
            try
            {
                var userId = _userManager.GetUserId(User); // Get the actual user Id
                if (userId == null)
                {
                    return Json(new { success = false, message = "User not authenticated." });
                }

                var submission = await _submissionService.SubmitExamAsync(userId, examId, answers);
                return Json(new { success = true, submissionId = submission.SubmissionId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"{ex.Message}. Inner Exception: {ex.InnerException?.Message}" });
            }
        }

        public async Task<IActionResult> ViewResult(int id)
        {
            var userId = _userManager.GetUserId(User) ?? "test-user"; // Use UserManager to get Id, with fallback
            var submission = await _unitOfWork.Repository<ExamSubmission>()
                .GetAllWithNestedIncludesAsync(q => q
                    .Include(es => es.Exam)
                    .Include(es => es.Answers)
                        .ThenInclude(a => a.Question)
                            .ThenInclude(q => q.Choices))
                .Result
                .FirstOrDefaultAsync(es => es.SubmissionId == id && es.UserId == userId);

            if (submission == null) return NotFound();

            return View(submission);
        }
    }
}