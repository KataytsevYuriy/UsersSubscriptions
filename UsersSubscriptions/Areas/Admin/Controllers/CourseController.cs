using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Areas.Admin.Models;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CourseController : Controller
    {
        private IAdminDataRepository repository;
        public CourseController(IAdminDataRepository repo)
        {
            repository = repo;
        }

        public IActionResult Index()
        {
            return View(repository.GetAllCourses());
        }

        public async Task<IActionResult> CourseDetails(string id)
        {
            return View(await repository.GetCourse(id));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeCourse(Course course)
        {
            await repository.UpdateCourseAsync(course);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditCourseTeachers(string id)
        {
            ViewBag.allTeschers = await repository.GetRoleUsersAsync("Teacher");
            return View(await repository.GetCourse(id));
        }
        [HttpPost]
        public async Task<IActionResult> EditCourseTeachers(Course course)
        {
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> DeleteCourse(string Id)
        {
            await repository.DeleteCourse(Id);
            return RedirectToAction(nameof(Index));
        }
    }
}