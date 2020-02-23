using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Teacher.Models;

namespace UsersSubscriptions.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class TeacherController : Controller
    {
        private ITeacherRepository repository;
        public TeacherController(ITeacherRepository repo) => repository = repo;
    
        public async Task<IActionResult> Index()
        {
            AppUser currentUser = await repository.GetCurrentUserAsync(HttpContext);
            return View(currentUser);
        }

        public async Task<IActionResult> TeacherCourses()
        {
            AppUser currentUser = await repository.GetCurrentUserAsync(HttpContext);
            //var ttt = await repository.GetTeacherCoursesAsync(currentUser);
            IEnumerable<Course> courses = await repository.GetTeacherCoursesAsync(currentUser);
            return View(courses);
        }

        public async Task<IActionResult> CourseInfo(string Id)
        {
            Course course = await repository.GetCoursInfoAsync(Id);
            return View(course);
        }
        public async Task<IActionResult> ConfirmSubscription(string Id)
        {
            Subscription subscription = await repository.GetSubscriptionAsync(Id);
            return View(subscription);
        }
        [HttpPost]
        public async Task<IActionResult> ConfirmSubscription(Subscription subsc)
        {
            AppUser currentUser = await repository.GetCurrentUserAsync(HttpContext);
            Subscription subscription = await repository.GetSubscriptionAsync(subsc.Id);
            if (subscription.ConfirmedByTeacher == null)
            {
                await repository.ConfirmSubscriptionAsync(currentUser, subsc.Id);
            } else
            {
                await repository.ConfirmPayedSubscriptionAsync(currentUser, subsc.Id);
            }
            return View(await repository.GetSubscriptionAsync(subsc.Id));
        }

        //[HttpPost]
        //public async Task<IActionResult> ConfirmPayedSubscription(Subscription subsc)
        //{
        //    AppUser currentUser = await repository.GetCurrentUserAsync(HttpContext);
        //    await repository.ConfirmPayedSubscriptionAsync(currentUser, subsc.Id);
        //    return RedirectToAction(nameof(ConfirmSubscription), subsc.Id);
        //}

    }
}