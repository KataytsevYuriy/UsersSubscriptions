using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;

namespace UsersSubscriptions.Controllers
{
    [Authorize(Roles = Common.UsersConstants.schoolOwner)]
    public class OwnerController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private ITeacherRepository repository;
        public OwnerController(ITeacherRepository repo, SignInManager<AppUser> signInManager)
        {
            repository = repo;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index()
        {
            string ownerId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (ownerId == null || string.IsNullOrEmpty(ownerId))
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction(nameof(Index));
            }
            AppUser curOwner = await repository.GetCurrentOwnerAsync(ownerId);
            if (curOwner == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction(nameof(Index));
            }
            IEnumerable<School> schools = curOwner.Schools;
            if (schools.Count() == 0)
            {
                return View(schools);
            }
            string subdomain = (HttpContext.GetRouteData().Values["subdomain"] ?? "").ToString();
            if (!string.IsNullOrEmpty(subdomain))
            {
                School school = schools.FirstOrDefault(sch => sch.UrlName.ToLower().Equals(subdomain.ToLower()));
                if (school == null)
                {
                    return View(new List<School>());
                }
                return RedirectToAction(nameof(SchoolInfo), new { id = school.Id });
            }
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
 


        public IActionResult AddCourse(string schoolId)
        {
            return View(new OwnerCourseViewModel { SchoolId = schoolId });
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(OwnerCourseViewModel course)
        {
            IdentityResult result = await repository.CreateCourseAsync(course);
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
            Course course = await repository.GetCoursAsync(id);
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
            Course course = await repository.GetCoursAsync(model.Id);
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
            }, model.TeachersId.Distinct<string>().ToList());
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс оновлено";
            }
            if (model.TeachersId.Distinct<string>().ToList()
                .Except(course.CourseAppUsers.Select(cour => cour.AppUserId).ToList())
                .Count() > 0)    //If teacher was added
            {
                return RedirectToAction(nameof(EditCourse), new { id = course.Id, schoolId = model.SchoolId });
            }
            return RedirectToAction(nameof(SchoolInfo), new { id = model.SchoolId });
        }


        public async Task<IActionResult> DeleteCourse(string id)
        {
            ViewBag.HasSubscriptions = await repository.CourseHasSubscriptions(id);
            return View(await repository.GetCoursAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(Course course)
        {
            var result = await repository.DeleteCourse(course.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс видалений";
                return RedirectToAction(nameof(SchoolInfo), new { id = course.SchoolId });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(SchoolInfo), new { id = course.SchoolId });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> GetUserByPhone(string id)
        {

            AppUser user = await repository.GetUserByPhone(id);
            if (user == null) { return Json(""); }
            return Json(new { id = user.Id, name = user.FullName });
        }

        [HttpPost]
        public async Task<JsonResult> GetUserById(string id)
        {
            AppUser user = await repository.GetUserAsync(id);
            if (user == null) { return Json(""); }
            return Json(new { id = user.Id, name = user.FullName });
        }
        [HttpPost]
        public async Task<JsonResult> AddTeacherToCourseAsync(string id, string courseId)
        {
            if (id == null || string.IsNullOrEmpty(id) || courseId == null || string.IsNullOrEmpty(courseId))
            {
                return Json("");
            }
            if (await repository.GetUserAsync(id) == null)
            {
                return Json("");
            }
            if (await repository.GetCoursAsync(courseId) == null)
            {
                return Json("");
            }
            await repository.AddTeacherToCourse(id, courseId);
            Course course = await repository.GetCoursAsync(courseId);
            List<AppUser> teachers = course.CourseAppUsers.Select(capu => capu.AppUser).ToList();
            string res = "";
            foreach (AppUser teacher in teachers)
            {
                res += " <input type=\"checkbox\" name=\"TeachersId\" value=\"" + teacher.Id
                     + "\" checked > " + teacher.FullName + "<br>";
            }
            return Json(res);
        }
    }
}