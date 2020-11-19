using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;
using UsersSubscriptions.DomainServices;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json;
using UsersSubscriptions.Services;

namespace UsersSubscriptions.Controllers
{
    [Authorize(Roles = Common.UsersConstants.schoolOwner + "," + Common.UsersConstants.admin)]
    public class OwnerController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUserService _userService;
        private ICourseService _courseService;
        private ITeacherService _teacherService;
        private ISchoolService _schoolService;
        private IPaymentService _paymentService;
        public OwnerController(SignInManager<AppUser> signInManager, IUserService userService,
                               ICourseService courseService, ITeacherService teacherService,
                               ISchoolService schoolService, IPaymentService paymentService)
        {
            _signInManager = signInManager;
            _userService = userService;
            _courseService = courseService;
            _teacherService = teacherService;
            _schoolService = schoolService;
            _paymentService = paymentService;
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
            AppUser curOwner = _teacherService.GetCurrentOwner(ownerId);
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



        public IActionResult SchoolDetails(string id, bool isItAdmin)
        {
            if (string.IsNullOrEmpty(id))
            {
                if (isItAdmin)
                {
                    return RedirectToAction("Index", "School");
                }
                else
                {
                    return RedirectToAction(nameof(Index), new { redirect = "SchoolDetails" });
                }
            }
            School school = _schoolService.GetSchool(id);
            if (school == null)
            {
                return NotFound();
            }
            CheckSchool checkSchool = new CheckSchool(_schoolService, HttpContext.Session);
            if (!isItAdmin && !checkSchool.IsSchoolAllowed(id))
            {
                return RedirectToAction(Common.UsersConstants.redirectPayPageAction, Common.UsersConstants.redirectPayPageController, new {schoolId=id });
            }
            if (!isItAdmin && !school.Enable)
            {
                return NotFound();
            }
            ViewBag.isItAdmin = isItAdmin;
            return View(school);
        }

        [HttpPost]
        public IActionResult SchoolDetails(School model, bool isItAdmin)
        {
            School school = _schoolService.GetSchool(model.Id);
            if (school == null)
            {
                return NotFound();
            }
            if (!isItAdmin && !school.Enable)
            {
                return NotFound();
            }
            if ((!isItAdmin && school.OwnerId != HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
                || (isItAdmin && !User.IsInRole(Common.UsersConstants.admin)))
            {
                return StatusCode(403);
            }
            IdentityResult result = _schoolService.UpdateSchoolOptions(model);
            if (!result.Succeeded)
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
                return View(model);
            }
            school = _schoolService.GetSchool(model.Id);
            ViewBag.isItAdmin = isItAdmin;
            return View(school);
        }


        public IActionResult AddCourse(string schoolId, bool isItAdmin)
        {
            return View("CourseDetails", new CourseViewModel
            {
                IsActive = true,
                SchoolId = schoolId,
                IsItAdmin = isItAdmin,
                IsCreatingNew = true,
                AllPaymentTypes = _schoolService.GetSchool(schoolId).PaymentTypes.ToList(),
            });
        }

        public IActionResult CourseDetails(string id, string schoolId, bool isItAdmin)
        {
            School school = _schoolService.GetSchool(schoolId);
            if (school == null)
            {
                return NotFound();
            }
            if (!isItAdmin && !school.Enable)
            {
                return NotFound();
            }
            Course course = _courseService.GetCourse(id);
            if (course == null)
            {
                return NotFound();
            }
            if ((!isItAdmin && course.School.OwnerId != HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier))
                || (isItAdmin && !User.IsInRole(Common.UsersConstants.admin)))
            {
                return StatusCode(403);
            }
            CourseViewModel model = _courseService.GetCourseViewModel(id);
            model.IsItAdmin = isItAdmin;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CourseDetails(CourseViewModel model)
        {
            IdentityResult result = new IdentityResult();
            if (model.IsCreatingNew)
            {
                result = await _courseService.CreateCourseAsync(model);
            }
            else
            {
                result = await _courseService.UpdateCourseAsync(model);
            }
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс збережено";
            }
            else
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return RedirectToAction(nameof(CourseDetails), new { id = model.Id, schoolId = model.SchoolId, isItAdmin = model.IsItAdmin });
        }


        [HttpPost]
        public async Task<IActionResult> DeleteCourse(CourseViewModel course)
        {
            var result = await _courseService.RemoveCourseAsync(course.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс видалений";
            }
            else
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId, isItAdmin=course.IsItAdmin });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetUserByPhone(string id)
        {
            AppUser user = _userService.GetUserByPhone(id);
            if (user == null) { return NotFound(); }
            return Json(new { id = user.Id, name = user.FullName });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(string id)
        {
            AppUser user = await _userService.GetUserAsync(id);
            if (user == null) { return NotFound(); }
            return Json(new { id = user.Id, name = user.FullName });
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult GetUserByName(string id)
        {
            IEnumerable<AppUser> appUsers = _userService.FindUserByName(id);
            if (appUsers == null || appUsers.Count() == 0) { return NotFound(); }
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
        public async Task<IActionResult> AddTeacherToCourseAsync(string id, string courseId)
        {
            if (id == null || string.IsNullOrEmpty(id) || courseId == null || string.IsNullOrEmpty(courseId))
            {
                return BadRequest();
            }
            if (await _userService.GetUserAsync(id) == null)
            {
                return NotFound();
            }
            if (_courseService.GetCourse(courseId) == null)
            {
                return NotFound();
            }
            await _teacherService.AddTeacherToCourse(id, courseId);
            Course course = _courseService.GetCourse(courseId);
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
            IdentityResult result = _paymentService
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
            CheckSchool checkSchool = new CheckSchool(_schoolService, HttpContext.Session);
            if (!checkSchool.IsSchoolAllowed(id))
            {
                return RedirectToAction(Common.UsersConstants.redirectPayPageAction, Common.UsersConstants.redirectPayPageController, new { schoolId = id });
            }
            SchoolCalculationsViewModel model = _schoolService.GetSchoolDetail(id, "", DateTime.Now, "", "");
            if (model == null)
            {
                return RedirectToAction(nameof(Index), new { redirect = "SchoolCalculation" });
            }
            return View(model);
        }

        [HttpPost]
        public IActionResult SchoolCalculation(SchoolCalculationsViewModel model)
        {
            SchoolCalculationsViewModel result = _schoolService
                .GetSchoolDetail(model.SchoolId, model.SelectedCourseId, model.Month, model.SelectedNavId, model.SelectedTeacherId);
            if (model == null)
            {
                return RedirectToAction(nameof(Index), new { redirect = "SchoolCalculation" });
            }
            return View(result);
        }
    }
}