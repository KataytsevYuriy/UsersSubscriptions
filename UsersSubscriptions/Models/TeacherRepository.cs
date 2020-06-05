using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UsersSubscriptions.Models.ViewModels;
using UsersSubscriptions.Data;
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

        //user

        public async Task<AppUser> GetUserAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public AppUser GetUserByPhone(string phone)
        {
            return _context.Users.FirstOrDefault(us => Regex.Replace(us.PhoneNumber, @"[^\d]+", "") == (phone));
        }

        public AppUser GetCurrentOwner(string userId)
        {
            AppUser currentOwner = _context.Users
                .Include(usr => usr.Schools)
                .FirstOrDefault(usr => usr.Id == userId);
            return currentOwner;
        }

        public IEnumerable<School> GetCurrentTeacherSchools(string userId)
        {
            List<School> curTeacherSchools = _context.Schools
                .Include(sch => sch.Courses).ThenInclude(cour => cour.CourseAppUsers)
                .Where(sch => sch.Courses.Any(cour => cour.CourseAppUsers.Any(cap => cap.AppUserId == userId)))
                .ToList();
            return curTeacherSchools;
        }


        public IEnumerable<AppUser> FindUserByName(string name)
        {
            name = name.ToLower();
            List<AppUser> appUsers = _context.Users.Where(user => user.FullName.ToLower().Contains(name)).ToList();
            return appUsers;
        }


        //course

        public Course GetCourse(string id)
        {
            return _context.Courses
                .Include(cour => cour.School).ThenInclude(sch => sch.Owner)
                .Include(cour => cour.CourseAppUsers).ThenInclude(appu => appu.AppUser)
                .FirstOrDefault(cour => cour.Id == id); ;
        }

        public async Task<IdentityResult> CreateCourseAsync(CourseViewModel model)
        {
            if (string.IsNullOrEmpty(model.SchoolId)) return IdentityResult.Failed(new IdentityError { Description = "Школа на задана" });
            if (string.IsNullOrEmpty(model.Name)) return IdentityResult.Failed(new IdentityError { Description = "Заповніть назву курсу" });
            Course course = _context.Courses
                .FirstOrDefault(cour => cour.Name == model.Name && cour.SchoolId == model.SchoolId);
            if (course != null) return IdentityResult.Failed(new IdentityError { Description = "Такий курс вже існує" });
            course = new Course();
            course.Name = model.Name;
            course.IsActive = model.IsActive;
            course.Price = model.Price;
            course.SchoolId = model.SchoolId;
            var state = _context.Courses.Add(course);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не доданий" });
            }
            await _context.SaveChangesAsync();
            course = _context.Courses
                .FirstOrDefault(cour => cour.Name == model.Name && cour.SchoolId == model.SchoolId);
            if (course == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не доданий" });
            }
            if (model.TeachersId.Count() > 0)
            {
                foreach (string teacherId in model.TeachersId)
                {
                    AppUser teacher = await _userManager.FindByIdAsync(teacherId);
                    if (teacher != null)
                    {
                        await _context.CourseAppUsers.AddAsync(new CourseAppUser
                        {
                            AppUserId = teacher.Id,
                            CourseId = course.Id,
                        });
                        if (!(await _userManager.IsInRoleAsync(teacher, Common.UsersConstants.teacher)))
                        {
                            await _userManager.AddToRoleAsync(teacher, Common.UsersConstants.teacher);
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateCourseAsync(CourseViewModel course)
        {
            Course dbCourse = _context.Courses.Include(cour => cour.CourseAppUsers)
                .FirstOrDefault(cour => cour.Id == course.Id);
            if (dbCourse == null)
                return IdentityResult.Failed(new IdentityError { Description = "Курс не знайдено" });
            IList<string> coursTeachersId = dbCourse.CourseAppUsers.Select(usr => usr.AppUserId).ToList();
            IEnumerable<string> addedTeachers = course.TeachersId.Except(coursTeachersId);
            IEnumerable<string> removedTeachers = coursTeachersId.Except(course.TeachersId);
            dbCourse.Name = course.Name;
            dbCourse.Description = course.Description;
            dbCourse.IsActive = course.IsActive;
            dbCourse.Price = course.Price;
            dbCourse.AllowOneTimePrice = course.AllowOneTimePrice;
            dbCourse.OneTimePrice = course.OneTimePrice;
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

        public IEnumerable<Course> GetTeacherCourses(string teacherId, string schoolId, bool onlyActive)
        {
            IEnumerable<Course> courses = _context.Courses
                .Include(cour => cour.School)
                .Include(cu => cu.CourseAppUsers)
                    .Where(cour =>
                        cour.CourseAppUsers.Any(dd => dd.AppUserId == teacherId)
                        && cour.SchoolId == schoolId
                    ).ToList();
            if (onlyActive)
            {
                courses = courses.Where(cour => cour.IsActive == true).ToList();
            }
            return courses;
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

        public async Task<IdentityResult> DeleteCourseAsync(string Id)
        {
            Course course = _context.Courses
                .Include(sub => sub.Subscriptions)
                .Include(teach => teach.CourseAppUsers)
                .FirstOrDefault(co => co.Id == Id);
            IEnumerable<string> deletingTeacherIds = course.CourseAppUsers.Select(cap => cap.AppUserId).ToList();
            if (course.CourseAppUsers.Count() > 0)
            {
                foreach (CourseAppUser courseAppUser in course.CourseAppUsers)
                {
                    CourseAppUser deletedCourseAppUser = _context.CourseAppUsers.FirstOrDefault(cap =>
                                  cap.AppUserId == courseAppUser.AppUserId && cap.CourseId == courseAppUser.CourseId);
                    if (deletedCourseAppUser != null)
                    {
                        _context.CourseAppUsers.Remove(courseAppUser);
                    }
                }
            }
            if (course.Subscriptions.Count() > 0)
            {
                foreach (Subscription sub in course.Subscriptions)
                {
                    Subscription subscription = _context.Subscriptions.FirstOrDefault(subs => subs.Id == sub.Id);
                    if (subscription != null)
                    {
                        _context.Remove(subscription);
                    }
                }
            }
            var state = _context.Courses.Remove(course);
            if (state.State == EntityState.Deleted)
            {
                if (course.CourseAppUsers.Count() > 0)
                {
                    foreach (CourseAppUser courseAppUser in course.CourseAppUsers)
                    {
                        AppUser deletingTeacher = await GetUserAsync(courseAppUser.AppUserId);
                        if (deletingTeacher != null
                            && (await _userManager.IsInRoleAsync(deletingTeacher, Common.UsersConstants.teacher)))
                        {
                            await _userManager.RemoveFromRoleAsync(deletingTeacher, Common.UsersConstants.teacher);
                        }
                    }
                }
                _context.SaveChanges();
                return IdentityResult.Success;
            }
            return IdentityResult.Failed(new IdentityError { Description = "Курс не видалений" });
        }

        public bool CourseHasSubscriptions(string id)
        {
            if ((_context.Courses
                .Include(cour => cour.Subscriptions)
                .FirstOrDefault(cour => cour.Id == id))
                .Subscriptions.Count() > 0)
            {
                return true;
            }
            return false;
        }



        //Subscriprions

        public IEnumerable<Subscription> GetUserSubscriptions(string userId, string schoolId, DateTime month)
        {
            return _context.Subscriptions
                .Include(cour => cour.Course).ThenInclude(us => us.CourseAppUsers).ThenInclude(use => use.AppUser)
                .Include(subs => subs.Course).ThenInclude(cour => cour.School)
                        .Where(sub =>
                            sub.AppUserId == userId
                            && sub.Month.Year == month.Year
                            && sub.Month.Month == month.Month
                            && sub.Course.SchoolId == schoolId
                        )
                        .ToList();
        }

        public async Task<IdentityResult> CreateSubscriptionAsync(Subscription subscription)
        {
            if (subscription.MonthSubscription
                && (_context.Subscriptions.FirstOrDefault(sub =>
                sub.AppUserId == subscription.AppUserId
                && sub.CourseId == subscription.CourseId
                && sub.Month.Year == subscription.Month.Year && sub.Month.Month == subscription.Month.Month
            )) != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Такий абонемент вже існує" });
            }
            var state = await _context.Subscriptions.AddAsync(subscription);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Абонемент не доданий" });
            }
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public void RemoveSubscription(string id)
        {
            Subscription subscription = _context.Subscriptions.FirstOrDefault(sub => sub.Id == id);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                _context.SaveChanges();
            }
        }


        //School
        public School GetSchool(string schoolId)
        {
            return _context.Schools
                .Include(sch => sch.Courses)
                .Include(sch => sch.Owner)
                .FirstOrDefault(sch => sch.Id == schoolId);
        }

        public School GetSchoolByUrl(string url)
        {
            return _context.Schools.FirstOrDefault(sch => sch.UrlName.ToLower().Equals(url.ToLower()));
        }

        public SchoolCalculationsViewModel GetSchoolDetail(string schoolId, string courseId,
            DateTime month, string selectedNavId, string selectedTeacherId)
        {
            if (string.IsNullOrEmpty(schoolId)) return new SchoolCalculationsViewModel();
            if (month.Year < 2000)
            {
                month = DateTime.Now;
            }
            IEnumerable<Course> courses = _context.Courses
                .Include(cour => cour.School)
                .Include(cour => cour.CourseAppUsers).ThenInclude(capu => capu.AppUser)
                .Include(cour => cour.Subscriptions).ThenInclude(sub => sub.AppUser)
                .Where(cour => cour.SchoolId == schoolId)
                .OrderBy(cour => cour.Name)
                .ToList();
            List<SchoolCourse> schoolCourses = new List<SchoolCourse>();
            if (courses != null && courses.Count() > 0)
            {
                schoolCourses = courses.Select(course => new SchoolCourse
                {
                    Id = course.Id,
                    Name = course.Name,
                    IsActive = course.IsActive,
                    Sum = course.Subscriptions
                     .Where(sub => sub.Month.Month == month.Month && sub.Month.Year == month.Year)
                     .Select(sub => sub.Price).Sum(),
                }).ToList();
            }
            List<Subscription> subscriptions = new List<Subscription>();
            if (!string.IsNullOrEmpty(courseId) && courses.FirstOrDefault(cour => cour.Id == courseId) != null)
            {
                subscriptions = courses.FirstOrDefault(cour => cour.Id == courseId)
                    .Subscriptions
                    .Where(sub => sub.Month.Month == month.Month && sub.Month.Year == month.Year)
                    /*.OrderBy(sub => sub.AppUser.FullName)*/.ToList();
            }
            IEnumerable<SchoolTeacher> schoolTeachers = courses.SelectMany(cour => cour.CourseAppUsers.Select(capu => capu.AppUser))
                .Distinct()
                .Select(usr => new SchoolTeacher
                {
                    Id = usr.Id,
                    FullName = usr.FullName,
                })
                .ToList();
            IEnumerable<Subscription> teacherSubscriptions = new List<Subscription>();
            if (!string.IsNullOrEmpty(selectedTeacherId))
            {
                IEnumerable<Course> teacherCourses = courses.Where(cour => cour.CourseAppUsers.Any(cap => cap.AppUserId == selectedTeacherId));
                teacherSubscriptions = teacherCourses.SelectMany(cour => cour.Subscriptions)
                    .Where(sub => sub.Month.Month == month.Month && sub.Month.Year == month.Year)
                    .ToList();
            }
            SchoolCalculationsViewModel model = new SchoolCalculationsViewModel
            {
                SchoolId = schoolId,
                SelectedCourseId = courseId,
                SelectedTeacherId = selectedTeacherId,
                Month = month,
                SelectedNavId = selectedNavId,
                SchoolCourses = schoolCourses,
                CourseSubscriptions = subscriptions,
                SchoolTeachers = schoolTeachers,
                TeacherSubscriptions = teacherSubscriptions,
            };
            return model;
        }

        public IEnumerable<School> GetUsersSchools(string userId)
        {
            return _context.Schools.Where(sch => sch.OwnerId == userId).ToList();
        }

        public IEnumerable<Student> GetTeacherMonthStudents(string courseId, DateTime month)
        {
            IEnumerable<Subscription> TeacherSubscriptions = _context.Subscriptions
                    .Include(us => us.AppUser)
                    .Where(cour => cour.CourseId == courseId
                        && cour.Month.Year == month.Year
                        && cour.Month.Month == month.Month)
                    .ToList();
            IList<Student> students = new List<Student>();
            foreach (var subscr in TeacherSubscriptions)
            {
                students.Add(new Student
                {
                    StudentName = subscr.AppUser == null ? subscr.FullName : subscr.AppUser.FullName,
                    Phone = subscr.AppUser == null ? subscr.Phone : subscr.AppUser.PhoneNumber,
                    Price = subscr.Price,
                });
            }
            return students;
        }

        public bool IsItThisSchoolOwner(string schoolId, string ownerId)
        {
            School school = _context.Schools.Include(sch => sch.Owner)
                .FirstOrDefault(sch => sch.Id == schoolId && sch.OwnerId == ownerId);
            return school == null ? false : true;
        }

        public bool IsItThisSchoolTeacher(string schoolId, string teacherId)
        {
            Course course = _context.Courses
                .Include(cour => cour.CourseAppUsers)
                .Include(cour => cour.School)
                .FirstOrDefault(cour =>
                cour.SchoolId == schoolId
                && cour.IsActive
                && cour.CourseAppUsers.Any(cau => cau.AppUserId == teacherId));
            return course == null ? false : true;
        }

    }
}
