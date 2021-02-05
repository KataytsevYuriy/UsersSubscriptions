using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;
using UsersSubscriptions.Services;
using UsersSubscriptions.DomainServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using System.Web;

namespace UsersSubscriptions.Controllers
{
    [Authorize(Roles = Common.UsersConstants.teacher + "," + Common.UsersConstants.schoolOwner + "," + Common.UsersConstants.admin)]
    public class TeacherController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IUserService _userService;
        private ITeacherService _teacherSecvice;
        private ICourseService _courseService;
        private ISchoolService _schoolService;
        private ISubscriptionsService _subscriptionsService;
        private IPaymentService _paymentService;
        public TeacherController(SignInManager<AppUser> signInManager,
            ITeacherService teacherSecvice, IUserService userService,
            ICourseService courseService, ISchoolService schoolService,
            ISubscriptionsService subscriptionsService, IPaymentService paymentService)
        {
            _signInManager = signInManager;
            _teacherSecvice = teacherSecvice;
            _userService = userService;
            _courseService = courseService;
            _schoolService = schoolService;
            _subscriptionsService = subscriptionsService;
            _paymentService = paymentService;
        }

        public IActionResult Index(string schoolId)
        {
            CheckSchool checkSchool = new CheckSchool(_schoolService, HttpContext.Session);
            string school_Id = checkSchool.GetSchoolId_From_Context((HttpContext.GetRouteData().Values["submomain"] ?? "").ToString());
            if (!string.IsNullOrEmpty(school_Id)) schoolId = school_Id;
            if (string.IsNullOrEmpty(schoolId)) return NotFound();
            if (!checkSchool.IsSchoolAllowed(schoolId))
            {
                return RedirectToAction(Common.UsersConstants.redirectPayPageAction, Common.UsersConstants.redirectPayPageController, new { schoolId });
            }
            ViewBag.schoolId = schoolId;
            IEnumerable<Course> teacherCourses = _courseService.GetTeacherCourses(
                    HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), schoolId, true);
            if (teacherCourses != null
                && teacherCourses.Where(cour => cour.AllowOneTimePrice == true).Count() > 0)
            {
                ViewBag.hasOneTimeCourses = true;
            }
            return View();
        }

        private async Task<IEnumerable<School>> SchoolFromContext()
        {
            string teacherId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            AppUser curTeacher = await _userService.GetUserAsync(teacherId);
            if (curTeacher == null)
            {
                await _signInManager.SignOutAsync();
                return new List<School>();
            }
            IEnumerable<School> schools = _teacherSecvice.GetCurrentTeacherSchools(teacherId);
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

        public async Task<IActionResult> SelectSchool(string redirectValue, string schoolId = "")
        {
            if (!string.IsNullOrEmpty(schoolId))
            {
                CheckSchool checkSchool = new CheckSchool(_schoolService, HttpContext.Session);
                string id = checkSchool.GetSchoolId_From_Context((HttpContext.GetRouteData().Values["subdomain"] ?? "").ToString(), schoolId);
                if (!string.IsNullOrEmpty(id)) return RedirectToAction(redirectValue);
            }
            ViewBag.redirectValue = redirectValue;
            IEnumerable<School> schools = new List<School>();
            schools = await SchoolFromContext();
            return View(schools);
        }

        public IActionResult AddOneTimeSubscription(string schoolId = "")
        {
            CheckSchool checkSchool = new CheckSchool(_schoolService, HttpContext.Session);
            string id = checkSchool.GetSchoolId_From_Context((HttpContext.GetRouteData().Values["subdomain"] ?? "").ToString());
            if (!string.IsNullOrEmpty(id)) schoolId = id;
            if (string.IsNullOrEmpty(schoolId)) return NotFound();
            IEnumerable<Course> teacherCourses = _courseService.GetTeacherCourses(
                    HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), schoolId, true);
            if (teacherCourses == null || teacherCourses.Where(cour => cour.AllowOneTimePrice == true).Count() == 0)
            {
                TempData["ErrorMessage"] = "У вас немає активних курсів з разовими абонементми";
                return RedirectToAction(nameof(Index), new { schoolId });
            }
            AddSubscriptionViewModel model = new AddSubscriptionViewModel()
            {
                TeacherCourses = teacherCourses,
                SchoolId = schoolId,
                Month = DateTime.Now,
                PaymentTypes = _paymentService.GetSchoolPaymentTyapes(schoolId),

            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddOneTimeSubscription(AddSubscriptionViewModel model)
        {
            CheckSchool checkSchool = new CheckSchool(_schoolService, HttpContext.Session);
            string schoolId = checkSchool.GetSchoolId_From_Context((HttpContext.GetRouteData().Values["subdomain"] ?? "").ToString()) ?? model.SchoolId;
            if (string.IsNullOrEmpty(schoolId))
                if (string.IsNullOrEmpty(schoolId = model.SchoolId)) return NotFound();
            Course course = _courseService.GetCourse(model.SelectedCours.Id);
            if (course == null) return NotFound();
            if (!course.AllowOneTimePrice)
            {
                TempData["ErrorMessage"] = "У цього курса відсутні разові абонементи";
                return RedirectToAction(nameof(AddOneTimeSubscription), new { schoolId });
            }
            if (course.CourseAppUsers.FirstOrDefault(cap =>
             cap.AppUserId == HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)) == null) return Forbid();
            if (string.IsNullOrEmpty(model.Student.FullName) && string.IsNullOrEmpty(model.Student.Id))
            {
                TempData["ErrorMessage"] = "Потрібно вибрати учня ";
                return RedirectToAction(nameof(AddOneTimeSubscription), new { schoolId });
            }
            if (string.IsNullOrEmpty(model.Student.Id) && (model.Student.FullName.Length < 5))
            {
                TempData["ErrorMessage"] = "Ім'я повинно бути довше 5 символів";
                return RedirectToAction(nameof(AddOneTimeSubscription), new { schoolId });
            }
            if (string.IsNullOrEmpty(model.SelectedPaymentType))
            {
                TempData["ErrorMessage"] = "Виберіть тип оплати";
                return RedirectToAction(nameof(AddOneTimeSubscription), new { schoolId });
            }
            DateTime now = DateTime.Now;
            if (model.Month.Date < DateTime.Now.Date)
            {
                TempData["ErrorMessage"] = "Виберіть коректну дату";
                return RedirectToAction(nameof(AddOneTimeSubscription), new { schoolId });
            }
            IEnumerable<Course> teacherCourses = _courseService.GetTeacherCourses(
                       HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), model.SchoolId, true);
            Subscription subscription = new Subscription
            {
                Id = Guid.NewGuid().ToString(),
                Period = model.Month,
                CourseId = model.SelectedCours.Id,
                MonthSubscription = false,
                CreatedDatetime = DateTime.Now,
                AppUserId=model.Student.Id,
                FullName=model.Student.FullName,
                Phone=model.Student.PhoneNumber,
                Payments = new List<Payment>
                {
                    new Payment
                    {
                        DateTime=DateTime.Now,
                        PaymentTypeId=model.SelectedPaymentType,
                        Price=model.SelectedCours.Price,
                        PayedToId=HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier),
                    },
                },
                Price = teacherCourses.FirstOrDefault(cour => cour.Id == model.SelectedCours.Id).OneTimePrice,
            };

            IdentityResult result = await _subscriptionsService.CreateSubscriptionAsync(subscription);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Відвідування додано";
            }
            else if (result.Errors.Count() > 0)
            {
                string erorMessage = "";
                foreach (var error in result.Errors)
                {
                    erorMessage += error.Description;
                }
                TempData["ErrorMessage"] = erorMessage;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> StudentInfo(string studentId, string schoolId, DateTime Month)
        {
            AppUser student = await _userService.GetUserAsync(studentId);
            DateTime curDate = DateTime.Now;
            if (student == null)
            {
                TempData["ErrorMessage"] = "Учня не знайдено";
                return NotFound();
            }
            if (Month.Year < 2000) Month = curDate;
            IEnumerable<Subscription> userSubscriptions = _subscriptionsService.GetUserSubscriptions(student.Id, schoolId, Month);
            StudentInfoViewModel model = new StudentInfoViewModel
            {
                Student = student,
                Subscriptions = userSubscriptions,
                Month = Month,
                ScholId = schoolId,
            };
            ViewBag.schoolOwner = _teacherSecvice.IsItThisSchoolOwner(schoolId, HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.curDate = curDate;
            ViewBag.teacherId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(model);
        }

        public async Task<IActionResult> AddSubscription(string Id, string schoolId, string month)
        {
            DateTime Month;
            DateTime.TryParse(month, out Month);
            if (Month.Year < 2000) { Month = DateTime.Now; }
            IEnumerable<Course> teacherCourses = _courseService.GetTeacherCourses(
                    HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), schoolId, true);
            IEnumerable<string> LastMonthUserCourses = _subscriptionsService.GetLastMonthUserCourseIds(Id);
            string selectedCourseId = teacherCourses.Select(cour => cour.Id).ToList().FirstOrDefault(id => LastMonthUserCourses.Contains(id));
            if (teacherCourses == null && teacherCourses.Where(cour => cour.AllowOneTimePrice == true).Count() == 0)
            {
                TempData["ErrorMessage"] = "У вас немає активних курсів";
                return RedirectToAction(nameof(StudentInfo), new { studentId = Id, schoolId, Month });
            }
            AddSubscriptionViewModel model = new AddSubscriptionViewModel
            {
                TeacherCourses = teacherCourses,
                Month = Month,
                SchoolId = schoolId,
                Student = await _userService.GetUserAsync(Id),
                PaymentTypes = _paymentService.GetSchoolPaymentTyapes(schoolId),
                SelectedCourseId = selectedCourseId,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubscription(AddSubscriptionViewModel model)
        {
            string teacherId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var subscription = new Subscription
            {
                AppUserId = model.Student.Id,
                CreatedDatetime = DateTime.Now,
                CourseId = model.SelectedCours.Id,
                Period = model.Month,
                MonthSubscription = true,
                Price = model.SelectedCours.Price,
            };
            IdentityResult result = await _subscriptionsService.CreateSubscriptionAsync(subscription);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Абонемент додано";
                return RedirectToAction(nameof(StudentInfo), new { studentId = model.Student.Id, schoolId = model.SchoolId, model.Month });
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

        public IActionResult SubscriptionDetails(string id)
        {
            Subscription subscription = _subscriptionsService.GetSubscription(id);
            return View(subscription);
        }

        [HttpPost]
        public IActionResult SubscriptionDetails(Subscription subscription)
        {
            string teacherId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            IdentityResult result = _subscriptionsService.UpdateSubscription(subscription, teacherId);
            return RedirectToAction(nameof(SubscriptionDetails), new { id = subscription.Id });
        }

        public IActionResult TeacherCourses(string schoolId)
        {
            CheckSchool checkSchool = new CheckSchool(_schoolService, HttpContext.Session);
            schoolId = checkSchool.GetSchoolId_From_Context((HttpContext.GetRouteData().Values["subdomain"] ?? "").ToString());
            if (string.IsNullOrEmpty(schoolId)) return NotFound();
            if (!checkSchool.IsSchoolAllowed(schoolId))
            {
                return RedirectToAction(Common.UsersConstants.redirectPayPageAction, Common.UsersConstants.redirectPayPageController, new { schoolId });
            }
            IEnumerable<Course> courses = _courseService.GetTeacherCourses(
                HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), schoolId, false);
            return View(courses);
        }

        public IActionResult RemoveSubscription(Subscription subscription)
        {
            _subscriptionsService.RemoveSubscription(subscription.Id);
            Course course = _courseService.GetCourse(subscription.CourseId);
            return RedirectToAction("StudentInfo", new { studentId = subscription.AppUserId, course.SchoolId, subscription.Period });
        }

        public IActionResult CourseInfo(string Id, DateTime month)
        {
            if (month.Year < 2000) { month = DateTime.Now; }
            IEnumerable<Subscription> subscriptions = _schoolService.GetTeacherMonthSubscriptions(Id, month);
            Course course = _courseService.GetCourse(Id);
            TeacherCoursesViewModel model = new TeacherCoursesViewModel
            {
                Subscriptions = subscriptions,
                Month = month,
                CurrentCourse = course
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

        public IActionResult PayForSchool(string schoolId)
        {
            School school = _schoolService.GetSchool(schoolId);
            return View(school);
        }

        private string GetSchoolId()
        {
            string subdomain = (HttpContext.GetRouteData().Values["subdomain"] ?? "").ToString();
            CheckSchool checkSchool = new CheckSchool(_schoolService, HttpContext.Session);
            return checkSchool.GetSchoolId_From_Context(subdomain);

        }

    }
}
