using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace UsersSubscriptions.Controllers
{
    [Authorize(Roles = Common.UsersConstants.teacher)]
    public class TeacherController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private ITeacherRepository teacherRepository;
        public TeacherController(ITeacherRepository repo, SignInManager<AppUser> signInManager)
        {
            teacherRepository = repo;
            _signInManager = signInManager;
        }

        public async Task<IActionResult> Index(string schoolId)
        {
            if (string.IsNullOrEmpty(schoolId))
            {
                IEnumerable<School> schools = await SchoolFromContext();
                if (schools.Count() > 1)
                {
                    return RedirectToAction(nameof(SelectSchool), new { redirectValue = "Index" });
                }
                if (schools.Count() == 0)
                {
                    return RedirectToAction("Index", "Home");
                }
                schoolId = schools.FirstOrDefault().Id;
            }
            ViewBag.schoolId = schoolId;
            return View();
        }

        private async Task<IEnumerable<School>> SchoolFromContext()
        {
            string teacherId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (teacherId == null || string.IsNullOrEmpty(teacherId))
            {
                await _signInManager.SignOutAsync();
                return new List<School>();
            }
            AppUser curTeacher = await teacherRepository.GetUserAsync(teacherId);
            if (curTeacher == null)
            {
                await _signInManager.SignOutAsync();
                return new List<School>();
            }
            IEnumerable<School> schools = teacherRepository.GetCurrentTeacherSchools(teacherId);
            if (schools.Count() == 0) return new List<School>();
            string subdomain = (HttpContext.GetRouteData().Values["subdomain"] ?? "").ToString();
            if (!string.IsNullOrEmpty(subdomain))
            {
                List<School> school = schools.Where(sch => sch.UrlName.ToLower().Equals(subdomain.ToLower())).ToList();
                if (school.Count() == 0) return new List<School>();
                return school;
            }
            return schools;
        }

        public async Task<IActionResult> SelectSchool(string redirectValue)
        {
            ViewBag.redirectValue = redirectValue;
            return View(await SchoolFromContext());
        }

        public IActionResult AddOneTimeSubscription(string schoolId)
        {
            return View();
        }

        public async Task<IActionResult> StudentInfo(string studentId, string schoolId, DateTime Month)
        {
            AppUser student = await teacherRepository.GetUserAsync(studentId);
            DateTime curDate = DateTime.Now;
            if (student == null)
            {
                TempData["ErrorMessage"] = "Учня не знайдено";
                return RedirectToAction(nameof(Index));
            }
            if (Month.Year < 2000) Month = curDate;
            IEnumerable<Subscription> userSubscriptions = teacherRepository.GetUserSubscriptions(student.Id, schoolId, Month);
            StudentInfoViewModel model = new StudentInfoViewModel
            {
                Student = student,
                Subscriptions = userSubscriptions,
                Month = Month,
                ScholId = schoolId,
            };
            ViewBag.curDate = curDate;
            return View(model);
        }

        public async Task<IActionResult> AddSubscription(string Id, string schoolId, string month)
        {
            DateTime Month;
            DateTime.TryParse(month, out Month);
            if (Month.Year < 2000) { Month = DateTime.Now; }
            AddSubscriptionViewModel model = new AddSubscriptionViewModel
            {
                TeacherCourses = teacherRepository.GetTeacherCourses(
                    HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), schoolId, true),
                Month = Month,
                SchoolId = schoolId,
                Student = await teacherRepository.GetUserAsync(Id),
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubscription(AddSubscriptionViewModel model)
        {
            AppUser teacher = await teacherRepository.GetUserAsync(HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            Subscription subscription = new Subscription
            {
                AppUserId = model.Student.Id,
                CreatedDatetime = DateTime.Now,
                CourseId = model.SelectedCours.Id,
                Month = model.Month,
                PayedDatetime = DateTime.Now,
                MonthSubscriprion = true,
                PayedToId = teacher.Id,
                Price = model.SelectedCours.Price,
            };
            IdentityResult result = await teacherRepository.CreateSubscriptionAsync(subscription);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Абонемент додано";
                return RedirectToAction(nameof(StudentInfo), new { studentId = model.Student.Id, schoolId = model.SchoolId, Month = model.Month });
            }
            if (result.Errors.Count() > 0)
            {
                string erorMessage = "";
                foreach (var error in result.Errors)
                {
                    erorMessage += error.Description;
                }
                TempData["ErrorMessage"] = erorMessage;
            }
            return RedirectToAction(nameof(AddSubscription), new { Id = model.Student.Id, schoolId = model.SchoolId, month = model.Month.ToString() });
        }

        public async Task<IActionResult> TeacherCourses(string schoolId)
        {
            if (string.IsNullOrEmpty(schoolId))
            {
                IEnumerable<School> schools = await SchoolFromContext();
                if (schools.Count() > 1)
                {
                    return RedirectToAction(nameof(SelectSchool), new { redirectValue = "TeacherCourses" });
                }
                else if (schools.Count() == 1)
                {
                    schoolId = schools.FirstOrDefault().Id;
                }
                else
                {
                    return View(new List<Course>());
                }
            }
            ViewBag.schoolId = schoolId;
            IEnumerable<Course> courses = teacherRepository.GetTeacherCourses(
                HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), schoolId, false);
            return View(courses);
        }

        public IActionResult CourseInfo(string Id, string schoolId, DateTime month)
        {
            if (month.Year < 2000) { month = DateTime.Now; }
            IEnumerable<Student> students = teacherRepository.GetTeacherMonthStudents(Id, month);
            Course course = teacherRepository.GetCourse(Id);
            TeacherCoursesViewModel model = new TeacherCoursesViewModel
            {
                Students = students,
                Month = month,
                CurrentCourse = course,
            };
            return View(model);
        }

        public JsonResult AddAvatar(string userId, IFormFile imageData)
        {
            if (!string.IsNullOrEmpty(userId) && imageData.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/avatars/avatar-" + userId + ".jpg");
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    imageData.CopyTo(fileStream);
                    return Json(true);
                }
            }
            return Json(false);
        }

    }
}
