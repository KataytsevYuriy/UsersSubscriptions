﻿using System;
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
using Newtonsoft.Json;

namespace UsersSubscriptions.Controllers
{
    [Authorize(Roles = Common.UsersConstants.teacher+","+Common.UsersConstants.schoolOwner)]
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
            IEnumerable<Course> teacherCourses = teacherRepository.GetTeacherCourses(
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

        public async Task<IActionResult> AddOneTimeSubscription(string schoolId)
        {
            if (string.IsNullOrEmpty(schoolId))
            {
                IEnumerable<School> schools = await SchoolFromContext();
                if (schools.Count() > 1)
                {
                    return RedirectToAction(nameof(SelectSchool), new { redirectValue = "AddOneTimeSubscription" });
                }
                if (schools.Count() == 0)
                {
                    return RedirectToAction("Index", "Home");
                }
                schoolId = schools.FirstOrDefault().Id;
            }
            IEnumerable<Course> teacherCourses = teacherRepository.GetTeacherCourses(
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
                PaymentTypes = teacherRepository.GetSchoolPaymentTyapes(schoolId),

            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddOneTimeSubscription(AddSubscriptionViewModel model)
        {
            IEnumerable<Course> teacherCourses = teacherRepository.GetTeacherCourses(
                       HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), model.SchoolId, true);
            if (teacherCourses == null || teacherCourses.Where(cour => cour.AllowOneTimePrice == true).Count() == 0)
            {
                TempData["ErrorMessage"] = "У вас немає активних курсів з разовими абонементми";
                return RedirectToAction(nameof(Index), new { model.SchoolId });
            }
            model.TeacherCourses = teacherCourses;
            Subscription subscription = new Subscription();
            if (model == null) return RedirectToAction(nameof(AddOneTimeSubscription));
            if (model.Student == null) return RedirectToAction(nameof(AddOneTimeSubscription));
            if (string.IsNullOrEmpty(model.Student.FullName) && string.IsNullOrEmpty(model.Student.Id))
            {
                TempData["ErrorMessage"] = "Потрібно вибрати учня ";
                return View(model);
            }
            if (string.IsNullOrEmpty(model.Student.Id) && (model.Student.FullName.Length < 5))
            {
                TempData["ErrorMessage"] = "Ім'я повинно бути довше 5 символів";
                return View(model);
            }
            //if(string.IsNullOrEmpty(model.Student.PhoneNumber))
            //{
            //    TempData["ErrorMessage"] = "Додайте телефон";
            //    return View(model);
            //}
            if (string.IsNullOrEmpty(model.SelectedPaymentType))
            {
                TempData["ErrorMessage"] = "Виберіть тип оплати";
                return View(model);
            }
            //if (string.IsNullOrEmpty(model.Student.Id) && (model.Student.PhoneNumber.Length < 17))
            //{
            //    TempData["ErrorMessage"] = "Введіть телефон повністю";
            //    return View(model);
            //}
            if (model.Month.Year < 2000) return RedirectToAction(nameof(AddOneTimeSubscription));
            if (model.SelectedCours == null || string.IsNullOrEmpty(model.SelectedCours.Id))
                return RedirectToAction(nameof(AddOneTimeSubscription));
            if (!string.IsNullOrEmpty(model.Student.Id))
            {
                subscription.AppUserId = model.Student.Id;
            }
            else if (!string.IsNullOrEmpty(model.Student.FullName))
            {
                subscription.FullName = model.Student.FullName;
                if (!string.IsNullOrEmpty(model.Student.PhoneNumber))
                {
                    subscription.Phone = model.Student.PhoneNumber;
                }
            }
            else return RedirectToAction(nameof(AddOneTimeSubscription));
            subscription.Month = model.Month;
            subscription.CourseId = model.SelectedCours.Id;
            subscription.PayedToId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            subscription.PayedDatetime = DateTime.Now;
            subscription.MonthSubscription = false;
            subscription.CreatedDatetime = DateTime.Now;
            subscription.Price = model.SelectedCours.Price;
            subscription.PaymentTypeId = model.SelectedPaymentType;

            IdentityResult result = await teacherRepository.CreateSubscriptionAsync(subscription);
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
            ViewBag.schoolOwner = teacherRepository.IsItThisSchoolOwner(schoolId, HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));
            ViewBag.curDate = curDate;
            ViewBag.teacherId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return View(model);
        }

        public async Task<IActionResult> AddSubscription(string Id, string schoolId, string month)
        {
            DateTime Month;
            DateTime.TryParse(month, out Month);
            if (Month.Year < 2000) { Month = DateTime.Now; }
            IEnumerable<Course> teacherCourses = teacherRepository.GetTeacherCourses(
                    HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier), schoolId, true);
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
                Student = await teacherRepository.GetUserAsync(Id),
                PaymentTypes = teacherRepository.GetSchoolPaymentTyapes(schoolId),
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubscription(AddSubscriptionViewModel model)
        {
            string teacherId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            Payment payment = new Payment
            {
                PayedToId = teacherId,
                Price = model.Payed,
                DateTime = DateTime.Now,
                PaymentTypeId = model.SelectedPaymentType,
            };
            Subscription subscription = new Subscription
            {
                AppUserId = model.Student.Id,
                CreatedDatetime = DateTime.Now,
                CourseId = model.SelectedCours.Id,
                Month = model.Month,
                PayedDatetime = DateTime.Now,
                MonthSubscription = true,
                Price = model.SelectedCours.Price,
                Payments = new List<Payment> {
                     new Payment
                            {
                                PayedToId = teacherId,
                                Price = model.Payed,
                                DateTime = DateTime.Now,
                                PaymentTypeId = model.SelectedPaymentType,
                            }
                },
            };
            IdentityResult result = await teacherRepository.CreateSubscriptionAsync(subscription);
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
            Subscription subscription = teacherRepository.GetSubscription(id);
            return View(subscription);
        }

        [HttpPost]
        public IActionResult SubscriptionDetails(Subscription subscription)
        {
            string teacherId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            IdentityResult result = teacherRepository.UpdateSubscription(subscription, teacherId);
            return RedirectToAction(nameof(SubscriptionDetails), new { id = subscription.Id });
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

        public IActionResult RemoveSubscription(Subscription subscription)
        {
            teacherRepository.RemoveSubscription(subscription.Id);
            Course course = teacherRepository.GetCourse(subscription.CourseId);
        //public async Task<IActionResult> StudentInfo(string studentId, string schoolId, DateTime Month)
            return RedirectToAction("StudentInfo", new { studentId=subscription.AppUserId, course.SchoolId, subscription.Month });
        }

        public IActionResult CourseInfo(string Id, string schoolId, DateTime month)
        {
            if (month.Year < 2000) { month = DateTime.Now; }
            IEnumerable<Subscription> subscriptions = teacherRepository.GetTeacherMonthSubscriptions(Id, month);
            Course course = teacherRepository.GetCourse(Id);
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

    }
}
