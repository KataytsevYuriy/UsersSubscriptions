using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;
using UsersSubscriptions.Data;
using System.Security.Principal;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace UsersSubscriptions.Models
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

        public async Task<AppUser> GetCurrentOwnerAsync(string userId)
        {
            AppUser currentOwner = await _context.Users
                .Include(usr => usr.Schools)
                .FirstOrDefaultAsync(usr => usr.Id == userId);
            return currentOwner;
        }

        public async Task<IEnumerable<School>> GetCurrentTeacherSchools(string userId)
        {
            List<School> curTeacherSchools = await _context.Schools
                .Include(sch => sch.Courses).ThenInclude(cour => cour.CourseAppUsers)
                .Where(sch => sch.Courses.Any(cour => cour.CourseAppUsers.Any(cap => cap.AppUserId == userId)))
                .ToListAsync();
            return curTeacherSchools;
        }

        public async Task<IEnumerable<Subscription>> GetUserSubscriptionsAsync(string userId, DateTime month)
        {
            return await _context.Subscriptions
                .Include(cour => cour.Course).ThenInclude(us => us.CourseAppUsers).ThenInclude(use => use.AppUser)
                        .Where(sub =>
                            sub.AppUserId == userId
                            && sub.Month.Year == month.Year
                            && sub.Month.Month == month.Month
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
                .Include(cu => cu.CourseAppUsers)
                    .Where(cap => cap.CourseAppUsers.Any(dd => dd.AppUserId == teacher.Id)).ToListAsync();
            return courses;
        }

        public async Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription)
        {
            if ((_context.Subscriptions.FirstOrDefault(sub =>
                sub.AppUserId == subscription.AppUserId
                && sub.CourseId == subscription.CourseId
                && sub.Month.Year == subscription.Month.Year && sub.Month.Month == subscription.Month.Month
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
                        && cour.Month.Year == month.Year
                        && cour.Month.Month == month.Month)
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
                .Include(cour => cour.School).ThenInclude(sch => sch.Owner)
                .Include(cour => cour.CourseAppUsers).ThenInclude(appu => appu.AppUser)
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
                return IdentityResult.Failed(new IdentityError { Description = "Введіть назву курсу" });
            if (string.IsNullOrEmpty(course.SchoolId))
                return IdentityResult.Failed(new IdentityError { Description = "Курс не додано" });
            var state = await _context.Courses.AddAsync(course);
            if (state.State != EntityState.Added)
                return IdentityResult.Failed(new IdentityError { Description = "Курс не додано" });
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateCourseAsync(Course course, IList<string> TeachersId)
        {
            Course dbCourse = await _context.Courses.Include(cour => cour.CourseAppUsers)
                .FirstOrDefaultAsync(cour => cour.Id == course.Id);
            if (dbCourse == null)
                return IdentityResult.Failed(new IdentityError { Description = "Курс не знайдено" });
            IList<string> coursTeachersId = dbCourse.CourseAppUsers.Select(usr => usr.AppUserId).ToList();
            IEnumerable<string> addedTeachers = TeachersId.Except(coursTeachersId);
            IEnumerable<string> removedTeachers = coursTeachersId.Except(TeachersId);
            dbCourse.Name = course.Name;
            dbCourse.Description = course.Description;
            dbCourse.IsActive = course.IsActive;
            dbCourse.Price = course.Price;
            var state = _context.Courses.Update(dbCourse);
            if (state.State != EntityState.Modified)
                return IdentityResult.Failed(new IdentityError { Description = "Курс не оновлено" });
            await _context.SaveChangesAsync();
            foreach (string teacher in addedTeachers)
            {
                AppUser user = await _userManager.FindByIdAsync(teacher);
                if (user != null)
                {
                    await _context.CourseAppUsers.AddAsync(new CourseAppUser
                    {
                        AppUserId = user.Id,
                        CourseId = dbCourse.Id,
                    });
                    if (!(await _userManager.IsInRoleAsync(user, Common.UsersConstants.teacher)))
                        await _userManager.AddToRoleAsync(user, Common.UsersConstants.teacher);
                }
            }
            foreach (string teacher in removedTeachers)
            {
                CourseAppUser courseAppUser = _context.CourseAppUsers
                    .FirstOrDefault(cau => cau.AppUserId == teacher && cau.CourseId == dbCourse.Id);
                if (courseAppUser != null)
                {
                    _context.CourseAppUsers.Remove(courseAppUser);
                    if (_context.CourseAppUsers.Where(teach => teach.AppUserId == teacher).Count() == 1)
                    {
                        AppUser user = await _userManager.FindByIdAsync(teacher);
                        if (user != null)
                            await _userManager.RemoveFromRoleAsync(user, Common.UsersConstants.teacher);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<AppUser> GetUserByPhone(string phone)
        {
            return await _context.Users.FirstOrDefaultAsync(us => Regex.Replace(us.PhoneNumber, @"[^\d]+", "") == (phone));
        }

        public async Task AddTeacherToCourse(string userId, string courseId)
        {
            if ((await _context.CourseAppUsers.FirstOrDefaultAsync(capu => capu.AppUserId == userId && capu.CourseId == courseId)) != null)
            {
                return;
            }
            await _context.CourseAppUsers.AddAsync(new CourseAppUser { AppUserId = userId, CourseId = courseId });
            await _context.SaveChangesAsync();
            AppUser user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                if (!(await _userManager.IsInRoleAsync(user, Common.UsersConstants.teacher)))
                {
                    await _userManager.AddToRoleAsync(user, Common.UsersConstants.teacher);
                }
            }

        }

    }
}
