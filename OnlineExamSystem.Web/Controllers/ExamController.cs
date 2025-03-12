using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineExamSystem.Domains.Entities;
using OnlineExamSystem.Web.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using OnlineExamSystem.BL.Abstraction;

namespace OnlineExamSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ExamController : Controller
    {
        private readonly IExamService _examService;

        public ExamController(IExamService examService)
        {
            _examService = examService;
        }

        public async Task<IActionResult> Index()
        {
            var exams = await _examService.GetAllExamsAsync();
            return View(await exams.ToListAsync());
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(ExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exam = new Exam
                {
                    Title = model.Title,
                    CreatedBy = User.Identity!.Name,
                    CreatedDate = DateTime.Now
                };
                await _examService.CreateExamAsync(exam);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var exams = await _examService.GetAllExamsAsync();
            var exam = await exams.FirstOrDefaultAsync(e => e.ExamId == id);
            if (exam == null) return NotFound();
            return View(new ExamViewModel { Title = exam.Title });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, ExamViewModel model)
        {
            if (ModelState.IsValid)
            {
                var exams = await _examService.GetAllExamsAsync();
                var exam = await exams.FirstOrDefaultAsync(e => e.ExamId == id);
                if (exam == null) return NotFound();
                exam.Title = model.Title;
                await _examService.EditExamAsync(exam);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _examService.DeleteExamAsync(id);
            return RedirectToAction("Index");
        }
    }
}