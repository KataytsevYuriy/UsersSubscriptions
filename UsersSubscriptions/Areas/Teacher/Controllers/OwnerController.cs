using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Teacher.Models;
using Microsoft.AspNetCore.Identity;

namespace UsersSubscriptions.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    [Authorize(Roles = Common.UsersConstants.schoolOwner)]
    public class OwnerController : Controller
    {
        private ITeacherRepository repository;
        public OwnerController(ITeacherRepository repo)
        {
            repository = repo;
        }

        public async Task<IActionResult> Index()
        {
            AppUser curUser = await repository.GetCurrentUserAsync(HttpContext);
            IEnumerable<School> schools = repository.GetUsersSchools(curUser.Id);
            if (schools.Count() == 1)
            {
                return RedirectToAction(nameof(SchoolInfo), new { id = schools.First().Id });
            }
            return View(schools);
        }

        public async Task<IActionResult> SchoolInfo(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(Index));
            }
            School school = await repository.GetSchoolAsync(id);
            if (school == null)
            {
                return RedirectToAction(nameof(Index));
            }
            return View(school);
        }
        public async Task<IActionResult> CourseInfo(string id, string schoolId)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(SchoolInfo), new { id = schoolId });
            }
            Course course = await repository.GetCoursInfoAsync(id);
            if (course==null)
            {
                TempData["ErrorMessage"] = "Курс не знайдено";
                return RedirectToAction(nameof(SchoolInfo), new { id = schoolId });
            }
            return View(course);
        }

        //public IActionResult EditCourse()

        public IActionResult AddCourse(string schoolId)
        {
            return View(new Course { SchoolId = schoolId });
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(Course course)
        {
            IdentityResult result = await repository.AddCourseAsync(course);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа додана";
                return RedirectToAction(nameof(SchoolInfo), new { id = course.SchoolId });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(AddCourse), new { schoolId = course.SchoolId });
        }


    }
}