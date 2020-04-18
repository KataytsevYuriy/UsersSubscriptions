using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Teacher.Models.ViewModels;
using UsersSubscriptions.Data;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;

namespace UsersSubscriptions.Areas.Teacher.Models
{
    public class TeacherRepository : ITeacherRepository
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public TeacherRepository(ApplicationDbContext ctx,
                                UserManager<AppUser> userMng,
                                RoleManager<IdentityRole> roleMng)
        {
            _context = ctx;
            _userManager = userMng;
            _roleManager = roleMng;
        }

        public async Task<AppUser> GetCurrentUserAsync(HttpContext context)
        {
            AppUser currentUser = await _userManager.GetUserAsync(context.User);
            return currentUser;
        }

        public async Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(string userId, DateTime month)
        {
            return await _context.Subscriptions
                .Include(cour => cour.Course).ThenInclude(us => us.CourseAppUsers).ThenInclude(use => use.AppUser)
                        .Where(sub =>
                            sub.AppUserId == userId
                            && sub.DayStart.Year == month.Year
                            && sub.DayStart.Month == month.Month
                        )
                        .ToListAsync();
        }

        public async Task<AppUser> GetUserAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<IEnumerable<Course>> GetTeacherCoursesAsync(AppUser teacher)
        {
            IEnumerable<Course> courses = await _context.Courses
                //.Include(sub => sub.Subscriptions)
                //.Include(cu => cu.Subscriptions).ThenInclude(cus => cus.ConfirmedBy)
                // .Include(cu => cu.Subscriptions).ThenInclude(cus => cus.PayedTo)
                .Include(cu => cu.CourseAppUsers)//.ThenInclude(te => te.AppUser)
                    .Where(cap => cap.CourseAppUsers.Any(dd => dd.AppUserId == teacher.Id)).ToListAsync();
            return courses;
        }

        public async Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription)
        {
            if ((_context.Subscriptions.FirstOrDefault(sub =>
                sub.AppUserId == subscription.AppUserId
                && sub.CourseId == subscription.CourseId
                && sub.DayStart.Year == subscription.DayStart.Year && sub.DayStart.Month == subscription.DayStart.Month
            )) != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Така підписка вже існує" });
            }
            var state = await _context.Subscriptions.AddAsync(subscription);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Підписка не додана" });
            }
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IEnumerable<Student>> GetTeacherMonthStudentsAsync(string courseId, DateTime month)
        {
            IEnumerable<Subscription> TeacherSubscriptions = await _context.Subscriptions
                    .Include(us => us.AppUser)
                    .Where(cour => cour.CourseId == courseId
                        && cour.DayStart.Year == month.Year
                        && cour.DayStart.Month == month.Month)
                    .ToListAsync();
            IList<Student> students = new List<Student>();
            foreach (var subscr in TeacherSubscriptions)
            {
                students.Add(new Student
                {
                    StudentName = subscr.AppUser.FullName,
                    Phone = subscr.AppUser.PhoneNumber,
                    Price = subscr.Price,
                });
            }
            return students;
        }

        public async Task<Course> GetCoursInfoAsync(string id)
        {
            return await _context.Courses
                .Include(cour=>cour.CourseAppUsers).ThenInclude(appu=>appu.AppUser)
                .FirstOrDefaultAsync(cour => cour.Id == id); ;
        }

        public IEnumerable<School> GetUsersSchools(string userId)
        {
            return _context.Schools.Where(sch => sch.OwnerId == userId).ToList();
        }

        public async Task<School> GetSchoolAsync(string schoolId)
        {
            return await _context.Schools
                .Include(sch => sch.Courses)
                .FirstOrDefaultAsync(sch => sch.Id == schoolId);
        }

        public async Task<IdentityResult> AddCourseAsync(Course course)
        {
            if (string.IsNullOrEmpty(course.Name))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Введіть назву курсу" });
            }
            if (string.IsNullOrEmpty(course.SchoolId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не додано" });
            }
            var state = await _context.Courses.AddAsync(course);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не додано" });
            }
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }
    }
}
