// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;

namespace OnlineExamSystem.Web.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return User.IsInRole("")
                    ? RedirectToAction("Index", "Exam")
                    : RedirectToAction("Index", "UserExam");
            }
            //return RedirectToAction("Login", "Account");
            return RedirectToAction("Login", "Account");


        }
    }
}