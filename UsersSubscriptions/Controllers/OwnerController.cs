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
using Newtonsoft.Json;

namespace UsersSubscriptions.Controllers
{
    [Authorize(Roles = Common.UsersConstants.schoolOwner)]
    public class OwnerController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private ITeacherRepository teacherRepository;
        public OwnerController(ITeacherRepository repo, SignInManager<AppUser> signInManager)
        {
            teacherRepository = repo;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index(string redirect)
        {
            if (string.IsNullOrEmpty(redirect))
            {
                return RedirectToAction("Index", "home");
            }
            string ownerId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(ownerId))
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "home");
            }
            AppUser curOwner = teacherRepository.GetCurrentOwner(ownerId);
            if (curOwner == null)
            {
                await _signInManager.SignOutAsync();
                return RedirectToAction("Index", "home");
            }
            IEnumerable<School> schools = curOwner.Schools;
            if (schools.Count() == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            string subdomain = (HttpContext.GetRouteData().Values["subdomain"] ?? "").ToString();
            if (!string.IsNullOrEmpty(subdomain))
            {
                School school = schools.FirstOrDefault(sch => sch.UrlName.ToLower().Equals(subdomain.ToLower()));
                if (school == null)
                {
                    return RedirectToAction("Index", "Home");
                }
                return RedirectToAction(redirect, new { id = school.Id });
            }
            if (schools.Count() == 1)
            {
                return RedirectToAction(redirect, new { id = schools.FirstOrDefault().Id });
            }
            return View(schools);
        }



        public IActionResult SchoolDetails(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(Index), new { redirect = "SchoolDetails" });
            }
            School school = teacherRepository.GetSchool(id);
            if (school == null)
            {
                return RedirectToAction(nameof(Index), new { redirect = "SchoolDetails" });
            }
            return View(school);
        }

        [HttpPost]
        public IActionResult SchoolSettings(School model, string redirectUrl)
        {
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                if (!User.IsInRole(Common.UsersConstants.admin))
                {
                    TempData["ErrorMessage"] = "Ви не адмін";
                    return RedirectToAction("Index", "home");
                }
            }
            else
            {
                if (!teacherRepository.IsItThisSchoolOwner(model.Id, HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)))
                {
                    TempData["ErrorMessage"] = "Ви не є власник школи";
                    return RedirectToAction("Index", "home");
                }
            }
            IdentityResult result = teacherRepository.UpdateSchoolOptions(model);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            if (!string.IsNullOrEmpty(redirectUrl) && redirectUrl.Equals("admin"))
            {
                return RedirectToAction("SchoolDetails", "School", new { Area = "Admin", id = model.Id });
            }
            School school = teacherRepository.GetSchool(model.Id);
            if (school == null)
            {
                TempData["ErrorMessage"] = "Школу не знайдено";
                return RedirectToAction("Index", "home");
            }
            
            return View("SchoolDetails", school);
        }


        public IActionResult AddCourse(string schoolId)
        {
            return View(new CourseViewModel { SchoolId = schoolId });
        }

        [HttpPost]
        public async Task<IActionResult> AddCourse(CourseViewModel course)
        {
            IdentityResult result = await teacherRepository.CreateCourseAsync(course);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа додана";
                return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(AddCourse), new { schoolId = course.SchoolId });
        }

        public IActionResult CourseDetails(string id, string schoolId)
        {
            Course course = teacherRepository.GetCourse(id);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Курс не знайдено";
                return RedirectToAction(nameof(SchoolDetails), new { id = schoolId });
            }
            if (course.School.OwnerId != HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return StatusCode(403);
            }
            CourseViewModel model = teacherRepository.GetCourseViewModel(id);
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> CourseDetails(CourseViewModel model)
        {
            IdentityResult result = await teacherRepository.UpdateCourseAsync(model);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс оновлено";
                return RedirectToAction(nameof(SchoolDetails), new { id = model.SchoolId });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(CourseDetails), new { id = model.Id, schoolId = model.SchoolId });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCourse(CourseViewModel course)
        {
            var result = await teacherRepository.DeleteCourseAsync(course.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс видалений";
            }
            else
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId });
        }

        [HttpPost]
        [AllowAnonymous]
        public JsonResult GetUserByPhone(string id)
        {
            AppUser user = teacherRepository.GetUserByPhone(id);
            if (user == null) { return Json(""); }
            return Json(new { id = user.Id, name = user.FullName });
        }

        [HttpPost]
        public async Task<JsonResult> GetUserById(string id)
        {
            AppUser user = await teacherRepository.GetUserAsync(id);
            if (user == null) { return Json(""); }
            return Json(new { id = user.Id, name = user.FullName });
        }

        [HttpPost]
        public JsonResult GetUserByName(string id)
        {
            IEnumerable<AppUser> appUsers = teacherRepository.FindUserByName(id);
            if (appUsers == null || appUsers.Count() == 0) { return Json(""); }
            List<UserJsonResponseModel> responseModel = new List<UserJsonResponseModel>();
            foreach (AppUser user in appUsers)
            {
                UserJsonResponseModel userJson = new UserJsonResponseModel
                {
                    Id = user.Id,
                    Name = user.FullName,
                };
                responseModel.Add(userJson);
            }
            var jsonResponse = JsonConvert.SerializeObject(responseModel);
            return Json(jsonResponse);
        }

        [HttpPost]
        public async Task<JsonResult> AddTeacherToCourseAsync(string id, string courseId)
        {
            if (id == null || string.IsNullOrEmpty(id) || courseId == null || string.IsNullOrEmpty(courseId))
            {
                return Json("");
            }
            if (await teacherRepository.GetUserAsync(id) == null)
            {
                return Json("");
            }
            if (teacherRepository.GetCourse(courseId) == null)
            {
                return Json("");
            }
            await teacherRepository.AddTeacherToCourse(id, courseId);
            Course course = teacherRepository.GetCourse(courseId);
            List<AppUser> teachers = course.CourseAppUsers.Select(capu => capu.AppUser).ToList();
            string res = "";
            foreach (AppUser teacher in teachers)
            {
                res += " <input type=\"checkbox\" name=\"TeachersId\" value=\"" + teacher.Id
                     + "\" checked > " + teacher.FullName + "<br>";
            }
            return Json(res);
        }

        [HttpPost]
        public JsonResult SetCoursePayTypes(string schoolId, string courseId, string[] pTypes)
        {
            IdentityResult result = teacherRepository
                .UpdateCoursePaymentTypes(schoolId, courseId, pTypes.ToList());
            if (result.Succeeded)
            {
                return Json("true");
            }
            return Json(result.Errors.FirstOrDefault().Description);
        }

        public IActionResult SchoolCalculation(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction(nameof(Index), new { redirect = "SchoolCalculation" });
            }
            SchoolCalculationsViewModel model = teacherRepository.GetSchoolDetail(id, "", DateTime.Now, "", "");
            if (model == null)
            {
                return RedirectToAction(nameof(Index), new { redirect = "SchoolCalculation" });
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult SchoolCalculation(SchoolCalculationsViewModel model)
        {
            SchoolCalculationsViewModel result = teacherRepository
                .GetSchoolDetail(model.SchoolId, model.SelectedCourseId, model.Month, model.SelectedNavId, model.SelectedTeacherId);
            if (model == null)
            {
                return RedirectToAction(nameof(Index), new { redirect = "SchoolCalculation" });
            }
            return View(result);
        }
    }
}