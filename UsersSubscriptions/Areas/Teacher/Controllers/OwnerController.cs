using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Teacher.Models;
using UsersSubscriptions.Areas.Teacher.Models.ViewModels;
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
            AppUser curUser = await repository.GetCurrentUserAsync(HttpContext);
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(SchoolInfo), new { id = schoolId });
            }
            Course course = await repository.GetCoursInfoAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Курс не знайдено";
                return RedirectToAction(nameof(SchoolInfo), new { id = schoolId });
            }
            if (course.School.OwnerId != curUser.Id)
            {
                return StatusCode(403);
            }
            return View(course);
        }

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

        public async Task<IActionResult> EditCourse(string id, string schoolId)
        {
            AppUser curUser = await repository.GetCurrentUserAsync(HttpContext);
            Course course = await repository.GetCoursInfoAsync(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Курс не знайдено";
                return RedirectToAction(nameof(SchoolInfo), new { id = schoolId });
            }
            if (course.School.OwnerId != curUser.Id)
            {
                return StatusCode(403);
            }
            OwnerCourseViewModel model = new OwnerCourseViewModel
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                IsActive = course.IsActive,
                Price = course.Price,
                CourseAppUsers = course.CourseAppUsers,
                SchoolId = course.SchoolId,
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> EditCourse(OwnerCourseViewModel model)
        {
            IdentityResult result;
            AppUser curUser = await repository.GetCurrentUserAsync(HttpContext);
            Course course = await repository.GetCoursInfoAsync(model.Id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Курс не знайдено";
                return RedirectToAction(nameof(SchoolInfo), new { id = model.SchoolId });
            }
            if (course.School.OwnerId != curUser.Id)
            {
                return StatusCode(403);
            }
            result = await repository.UpdateCourseAsync(new Course
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                IsActive = model.IsActive,
                Price = model.Price,
            }, model.Teachers.Distinct<string>().ToList());
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс оновлено";
            }
            return RedirectToAction(nameof(CourseInfo), new { id = course.Id, schoolId = course.SchoolId });
        }

        [HttpPost]
        public async Task<JsonResult> GetUserByPhone(string id)
        {
            AppUser user = await repository.GetUserByPhone(id);
            if (user == null) { return Json(""); }
            string result = "<input type=\"checkbox\" name=\"Teachers\" value=\"" + user.Id + "\" checked />" + user.FullName +"<br/>";
            return Json(result);
        }

        [HttpPost]
        public async Task<JsonResult> GetUserById(string id)
        {
            AppUser user = await repository.GetUserAsync(id);
            if (user == null) { return Json(""); }
            string result = "<input type=\"checkbox\" name=\"Teachers\" value=\"" + user.Id + "\" checked />" + user.FullName + "<br/>";
            return Json(result);
        }

    }
}