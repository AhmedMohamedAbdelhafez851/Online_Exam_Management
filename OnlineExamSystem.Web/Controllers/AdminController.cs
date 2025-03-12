using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamSystem.BL.Abstraction;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Web.Models.ViewModels;

namespace OnlineExamSystem.Web.Controllers
{
    // [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IExamService _examService;

        public AdminController(IExamService examService) => _examService = examService;

        public async Task<IActionResult> Index() => View(await _examService.GetAllExamsAsync());

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(ExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exam = new Exam
                {
                    Title = model.Title,
                    CreatedBy = User.Identity!.Name!,
                    CreatedDate = DateTime.Now
                };
                await _examService.CreateExamAsync(exam);
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}
