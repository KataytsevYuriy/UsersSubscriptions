using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Teacher.Models;

namespace UsersSubscriptions.Areas.Teacher.Controllers
{
    [Area("Teacher")]
    public class TeacherController : Controller
    {
        private ITeacherRepository repository;
        private IUserRepository userRepository;
        public TeacherController(ITeacherRepository repo, IUserRepository userRepo)
        {
            repository = repo;
            userRepository = userRepo;
        }
    
        public async Task<IActionResult> Index()
        {
            AppUser currentUser = await repository.GetCurrentUserAsync(HttpContext);
            return View(currentUser);
        }

        public async Task<IActionResult> TeacherCourses()
        {
            AppUser currentUser = await repository.GetCurrentUserAsync(HttpContext);
            IEnumerable<Course> courses = await repository.GetTeacherCoursesAsync(currentUser);
            return View(courses);
        }

        public async Task<IActionResult> CourseInfo(string Id)
        {
            Course course = await repository.GetCoursInfoAsync(Id);
            return View(course);
        }

        public async Task<IActionResult> ConfirmSubscription(string Id)
        {
            Subscription subscription = await repository.GetSubscriptionAsync(Id);
            return View(subscription);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmSubscription(Subscription subsc)
        {
            AppUser currentUser = await repository.GetCurrentUserAsync(HttpContext);
            Subscription subscription = await repository.GetSubscriptionAsync(subsc.Id);
            if (subscription.ConfirmedBy == null)
            {
                await repository.ConfirmSubscriptionAsync(currentUser, subsc.Id);
            } else
            {
                await repository.ConfirmPayedSubscriptionAsync(currentUser, subsc.Id);
            }
            return View(await repository.GetSubscriptionAsync(subsc.Id));
        }

        public IActionResult ScanQrCode()
        {
            return View();
        }

         public async Task<IActionResult> RemoveSubscription(string Id)
        {
            return View(await repository.GetSubscriptionAsync(Id));
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSubscription(Subscription subscription)
        {
            if (await repository.GetSubscriptionAsync(subscription.Id) != null)
            {
                await repository.RemoveSubscriptionAsync(subscription.Id);
            }
            return RedirectToAction(nameof(CourseInfo),new { Id=subscription.CourseId});
        }

       // [HttpPost]
        public async Task<IActionResult> StudentInfo(string studentQR)
        {
            AppUser student = await repository.GetUserAsync(studentQR);
            if (student == null)
            {
                return RedirectToAction(nameof(ScanQrCode));
            }
            IEnumerable<Course> teacherCourses = await repository.GetTeacherCoursesAsync(await repository.GetCurrentUserAsync(HttpContext));
            teacherCourses = teacherCourses.Where(cour => cour.IsActive == true);
            StudentInfoViewModel model = new StudentInfoViewModel
            {
                Student = student,
                Courses = teacherCourses
            };
            return View(model);
        }

        public async Task<IActionResult> CourseStudentInfo(string studentId, string courseId)
        {
            IEnumerable<Subscription> studentSubscriptions = repository.GetStudentSubscriptionsOfCourse(studentId, courseId);
            AppUser student = await repository.GetUserAsync(studentId);
            Course course = await repository.GetCoursInfoAsync(courseId);
            CourceStudentViewModel model = new CourceStudentViewModel
            {
                Course = course,
                Student = student,
                Subscriptions = studentSubscriptions
            };
            return View(model);
        }

        public async Task<IActionResult> AddSubscription(string studentId, string courseId)
        {
            SubscriptionTeatcherViewModel model = new SubscriptionTeatcherViewModel
            {
                Student = await repository.GetUserAsync(studentId),
                Course = await repository.GetCoursInfoAsync(courseId)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddSubscription(SubscriptionTeatcherViewModel model)
        {
            AppUser teacher = await repository.GetCurrentUserAsync(HttpContext);
            Subscription subscription = new Subscription {
                AppUserId = model.Student.Id,
                ConfirmedDatetime = DateTime.Now,
                CreatedDatetime = DateTime.Now,
                ConfirmedById = teacher.Id,
                CourseId = model.Course.Id,
                DayStart=model.Subscription.DayStart,
        };
            await userRepository.CreateSubscription(subscription);
            return RedirectToAction(nameof(StudentInfo), new { studentQR = model.Student.Id});
        }

         public async Task<IActionResult> RemoveStudentSubscription(string Id)
        {
            Subscription subscription = await repository.GetSubscriptionAsync(Id);
            return View(subscription);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveStudentSubscription(Subscription model)
        {
            Subscription subscription = await repository.GetSubscriptionAsync(model.Id);
            await repository.RemoveSubscriptionAsync(model.Id);
            return RedirectToAction(nameof(StudentInfo), new { studentQR = subscription.AppUserId });
        }
    }
}