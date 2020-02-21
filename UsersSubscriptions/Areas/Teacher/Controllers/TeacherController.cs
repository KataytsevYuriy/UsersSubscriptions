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
            return View(await repository.GetTeacherCoursesAsync(currentUser));
        }
    }
}