using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Areas.Admin.Models;
using UsersSubscriptions.Areas.Admin.Models.ViewModels;
using UsersSubscriptions.Common;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UsersConstants.admin)]
    public class SchoolController : Controller
    {
        private IAdminDataRepository repository;
        private ITeacherRepository repositoryTeacher;
        public SchoolController(IAdminDataRepository repo, ITeacherRepository repoTeacher)
        {
            repository = repo;
            repositoryTeacher = repoTeacher;
        }

        public IActionResult Index()
        {
            return View(repository.GetAllSchools());
        }

        public IActionResult CreateSchool()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchool(School school)
        {
            IdentityResult result = await repository.CreateSchoolAsync(school);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа додана";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(CreateSchool));
        }

        public IActionResult SchoolDetails(string id)
        {
            School school = repositoryTeacher.GetSchool(id);
            if (school == null)
            {
                TempData["ErrorMessage"] = "Школа не знайдена";
                return RedirectToAction(nameof(Index));
            }
            return View(school);
        }

        [HttpPost]
        public async Task<IActionResult> SchoolDetails(School school)
        {
            IdentityResult result = await repository.UpdateSchoolAsync(school);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа оновлена";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(SchoolDetails), new { id = school.Id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSchool(School school)
        {
            IdentityResult result = await repository.DeleteScoolAsync(school.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа видалена";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateCourse(string id)
        {
            return View(new CourseViewModel { SchoolId = id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseViewModel course)
        {
            IdentityResult result = await repositoryTeacher.CreateCourseAsync(course);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс доданий";
                return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return View(course);
        }

        public IActionResult CourseDetails(string id, string schoolId)
        {
            CourseViewModel model = repositoryTeacher.GetCourseViewModel(id);
            if (model == null)
            {
                TempData["ErrorMessage"] = "Курс не знайдено";
                return RedirectToAction(nameof(SchoolDetails), new { id = schoolId });
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourse(CourseViewModel course)
        {
            IdentityResult result = await repositoryTeacher.UpdateCourseAsync(course);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс оновлений";
                return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(CourseDetails), new { id = course.Id });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(Course course)
        {
            IdentityResult result = await  repositoryTeacher.DeleteCourseAsync(course.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс видалений";
            } else
            {
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeSchoolOwnerAsync(string id, string schoolId)
        { 
            if (id == null || string.IsNullOrEmpty(id)
                || schoolId == null || string.IsNullOrEmpty(schoolId))
            {
                return Json("");
            }
            if (await repositoryTeacher.GetUserAsync(id) == null)
            {
                return Json("");
            }
            if ( repositoryTeacher.GetSchool(schoolId) == null)
            {
                return Json("");
            }
            IdentityResult result = await repository.ChengeOwnerAsync(id, schoolId);
            if (!result.Succeeded)
            {
                return Json("");
            }
            return Json("true");
        }
    }
}