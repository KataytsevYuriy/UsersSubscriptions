using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Controllers
{
    public class StudentController : Controller
    {
        private IUserRepository repository;
        public StudentController(IUserRepository repo) => repository = repo;
 
        public async Task<IActionResult> Index()
        {
            AppUser currentUser = await repository.GetCurentUser(HttpContext);
            return View(currentUser);
        }

        public async Task<IActionResult> AddToCourse(string id)
        {
            ViewBag.course = await repository.GetCourse(id);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCourse(Subscription subscription)
        {
            AppUser currentUser = await repository.GetCurentUser(HttpContext);
            subscription.AppUser = currentUser;
            subscription.CreatedDatetime = DateTime.Now;
            await repository.CreateSubscription(subscription);
            return RedirectToAction(nameof(UserCourses));
        }

        public async Task<IActionResult> UserCourses()
        {
            AppUser currentUser = await repository.GetCurentUser(HttpContext);
            AppUser appUser = repository.GetUserCourses(currentUser.Id);
            return View(appUser);
        }

        public IActionResult AllCourses()
        {
            return View(repository.GetAllCourses());
        }

    }
}