﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace UsersSubscriptions.Controllers
{
    [Authorize(Roles = Common.UsersConstants.teacher)]
    public class TeacherController : Controller
    {
        private ITeacherRepository repository;
        public TeacherController(ITeacherRepository repo)
        {
            repository = repo;
        }
    
        public IActionResult Index()
        {
            return View();
        }

        // [HttpPost]
        public async Task<IActionResult> StudentInfo(string studentId, DateTime Month)
        {
            AppUser student = await repository.GetUserAsync(studentId);
            DateTime curDate = DateTime.Now;
            if (student == null)
            {
                return RedirectToAction(nameof(Index));
            }
            if (Month.Year < 2000)
            {
                Month = curDate;
            }
            IEnumerable<Subscription> userSubscriptions = await repository.GetUserSubscriptionsAsync(student.Id, Month);
            StudentInfoViewModel model = new StudentInfoViewModel
            {
                Student = student,
                Subscriptions = userSubscriptions,
                Month=Month
            };
            ViewBag.curDate = curDate;
            return View(model);
        }

        public async Task<IActionResult> AddSubscription(string Id, string month)
        {
            DateTime Month;
            DateTime.TryParse(month,out Month);
           // Course curCours;
            if (Month.Year < 2000) { Month = DateTime.Now; }
            AddSubscriptionViewModel model = new AddSubscriptionViewModel
            {
                Student = await repository.GetUserAsync(Id),
                TeacherCourses = await repository.GetTeacherCoursesAsync(await repository.GetCurrentUserAsync(HttpContext)),
                Month = Month,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubscription(AddSubscriptionViewModel model)
        {
            AppUser teacher = await repository.GetCurrentUserAsync(HttpContext);
            Subscription subscription = new Subscription
            {
                AppUserId = model.Student.Id,
                CreatedDatetime = DateTime.Now,
                CourseId = model.SelectedCours.Id,
                Month = model.Month,
                PayedDatetime = DateTime.Now,
                PayedToId = teacher.Id,
                Price = model.SelectedCours.Price,
            };
            IdentityResult result = await repository.CreateSubscriptionAsync(subscription);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] ="Підписку додано";
                return RedirectToAction(nameof(StudentInfo), new { studentId = model.Student.Id, Month = model.Month });
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
            return RedirectToAction(nameof(AddSubscription), new { Id = model.Student.Id, month = model.Month.ToString()});
        }

        public async Task<IActionResult> TeacherCourses()
        {
            AppUser currentUser = await repository.GetCurrentUserAsync(HttpContext);
            IEnumerable<Course> courses = await repository.GetTeacherCoursesAsync(currentUser);
            return View(courses);
        }

        public async Task<IActionResult> CourseInfo(string Id, DateTime month)
        {
            AppUser currentUser = await repository.GetCurrentUserAsync(HttpContext);
            if (month.Year < 2000) { month = DateTime.Now; }
            IEnumerable<Course> courses = await repository.GetTeacherCoursesAsync(currentUser);
            IEnumerable<Student> students = await repository.GetTeacherMonthStudentsAsync(Id, month);
            Course course = await repository.GetCoursInfoAsync(Id);
            TeacherCoursesViewModel model = new TeacherCoursesViewModel
            {
                Students = students,
                TeacherCourses = courses,
                Month = month,
                CurrentCourse = course,
            };
            return View(model);
        }
 
    }
}