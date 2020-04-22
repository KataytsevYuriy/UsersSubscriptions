using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Areas.Admin.Models;
using UsersSubscriptions.Models;
using UsersSubscriptions.Common;
using Microsoft.AspNetCore.Authorization;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =UsersConstants.admin)]
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

        public async Task<IActionResult> CreateCourse()
        {
            CourseViewModel model = new CourseViewModel
            {
                AllTeachers = await repository.GetRoleUsersAsync(UsersConstants.teacher)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse (CourseViewModel course)
        {
            await repository.CreateCourseAsync(course);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> CourseDetails(string id)
        {
            Course course = await repository.GetCourseAsync(id);
            IList<AppUser> appUsers = course.CourseAppUsers.Select(i=>i.AppUser).ToList();
            IList<AppUser> allTeachers = await repository.GetRoleUsersAsync(UsersConstants.teacher);
             CourseViewModel model = new CourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                IsActive = course.IsActive,
                Price = course.Price,
                Teachers = appUsers,
                AllTeachers = allTeachers
            };
            return View(model);
        }

        public async Task<IActionResult> DeleteCourse(string Id)
        {
            await repository.DeleteCourse(Id);
            return RedirectToAction(nameof(Index));
        }
    }
}