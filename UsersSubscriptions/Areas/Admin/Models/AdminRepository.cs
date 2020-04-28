using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Admin.Models.ViewModels;
using UsersSubscriptions.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace UsersSubscriptions.Areas.Admin.Models
{
    public class AdminRepository : IAdminDataRepository
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public AdminRepository(ApplicationDbContext ctx,
                                UserManager<AppUser> manager,
                                RoleManager<IdentityRole> roleManager)
        {
            _userManager = manager;
            _roleManager = roleManager;
            _context = ctx;
        }

        //user
        public IEnumerable<AppUser> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public async Task<AppUser> GetUserAsync(string id)
        {
            return await _userManager.FindByIdAsync(id);
        }

        public async Task UpdateUserAsync(AppUser user, IList<string> newUserRoles)
        {
            AppUser dbUser = await _userManager.FindByIdAsync(user.Id);
            if (dbUser != null)
            {
                dbUser.FullName = user.FullName;
                dbUser.Email = user.Email;
                dbUser.PhoneNumber = user.PhoneNumber;
                dbUser.IsActive = user.IsActive;
                await _userManager.UpdateAsync(dbUser);
                IList<string> userRoles = await GetUserRolesAsync(user.Id);
                IEnumerable<string> addedRoles = newUserRoles.Except(userRoles);
                var removedRoles = userRoles.Except(newUserRoles);
                await _userManager.AddToRolesAsync(dbUser, addedRoles);
                await _userManager.RemoveFromRolesAsync(dbUser, removedRoles);
            }
        }

        public async Task DeleteUseAsyncr(string id)
        {
            IdentityResult result = new IdentityResult();
            AppUser user = await _context.Users
                                .Include(s => s.CourseAppUsers)
                                .Include(s => s.Subscriptions)
                                .Include(s => s.SubscriptionPayedTo)
                                .FirstOrDefaultAsync(us => us.Id == id);
            if (user != null)
            {
                if (user.CourseAppUsers.Count() == 0)
                {
                    if (user.Subscriptions.Count() == 0)
                    {
                        if (user.SubscriptionPayedTo.Count() == 0)
                        {
                            IList<string> roles = await GetUserRolesAsync(id);
                            if (roles.Count > 0)
                            {
                                await _userManager.RemoveFromRolesAsync(user, roles);
                            }
                            result = await _userManager.DeleteAsync(user);
                        }
                        // Trow exception if Payed to this user, confirmed...                  
                    }
                }
                else   //user.CourseAppUsers.Count() >0
                {
                    result = IdentityResult.Failed(new IdentityError { Description = "CourseAppUsers.Count() >0" });
                }

            }
        }

        //role
        public List<IdentityRole> GetAllRoles()
        {
            return _roleManager.Roles.ToList();
        }

        public async Task<IList<string>> GetUserRolesAsync(string id)
        {
            return await _userManager.GetRolesAsync(new AppUser { Id = id });
        }

        public async Task CreateRoleAsync(IdentityRole role)
        {
            if (!(await _roleManager.RoleExistsAsync(role.Name)))
            {
                await _roleManager.CreateAsync(role);
            }
        }

        public async Task<IdentityRole> GetRoleAsync(string id)
        {
            return await _roleManager.FindByIdAsync(id);
        }

        public async Task<IList<AppUser>> GetRoleUsersAsync(string roleName)
        {
            return await _userManager.GetUsersInRoleAsync(roleName);
        }

        public async Task UpdateRole(IdentityRole model)
        {
            IdentityRole role = await GetRoleAsync(model.Id);
            if (role != null)
            {
                role.Name = model.Name;
                await _roleManager.UpdateAsync(role);
                await _roleManager.UpdateAsync(role);
            }
        }

        public async Task DeleteRoleAsync(string id)
        {
            IdentityRole role = await GetRoleAsync(id);
            if (role != null)
            {
                if ((await GetRoleUsersAsync(role.Name)).Count == 0)
                {
                    await _roleManager.DeleteAsync(role);
                }
            }
        }

        //Courses
        public IEnumerable<Course> GetAllCourses()
        {
            return _context.Courses.Include(p => p.CourseAppUsers).ThenInclude(usr => usr.AppUser).ToList();
        }

        public async Task<IdentityResult> CreateCourseAsync(Course model)
        {
            if (model.SchoolId == null || string.IsNullOrEmpty(model.SchoolId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа на задана" });
            }
            if (string.IsNullOrEmpty(model.Name))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Заповніть назву курсу" });
            }
            Course course = await _context.Courses
                .FirstOrDefaultAsync(cour => cour.Name == model.Name && cour.SchoolId == model.SchoolId);
            if (course != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Такий курс вже існує" });
            }
            course = new Course();
            course.Name = model.Name;
            course.IsActive = model.IsActive;
            course.Price = model.Price;
            course.SchoolId = model.SchoolId;
            var state = await _context.Courses.AddAsync(course);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не доданий" });
            }
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> CreateCourseAsync(CourseDetailsViewModel model)
        {
            if (string.IsNullOrEmpty(model.SchoolId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа на задана" });
            }
            if (string.IsNullOrEmpty(model.Name))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Заповніть назву курсу" });
            }
            Course course = await _context.Courses
                .FirstOrDefaultAsync(cour => cour.Name == model.Name && cour.SchoolId == model.SchoolId);
            if (course != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Такий курс вже існує" });
            }
            course = new Course();
            course.Name = model.Name;
            course.IsActive = model.IsActive;
            course.Price = model.Price;
            course.SchoolId = model.SchoolId;
            var state = await _context.Courses.AddAsync(course);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не доданий" });
            }
            await _context.SaveChangesAsync();
            course = await _context.Courses
                .FirstOrDefaultAsync(cour => cour.Name == model.Name && cour.SchoolId == model.SchoolId);
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

        public async Task<Course> GetCourseAsync(string id)
        {
            return await _context.Courses.Include(p => p.CourseAppUsers).ThenInclude(u => u.AppUser)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IdentityResult> UpdateCourseAsync(CourseDetailsViewModel course)
        {
            Course dbCourse = await GetCourseAsync(course.Id);
            if (dbCourse == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не знайдено" });
            }
            IList<string> coursTeachersId = dbCourse.CourseAppUsers.Select(usr => usr.AppUserId).ToList();
            IEnumerable<string> addedTeachers = course.TeachersId.Except(coursTeachersId);
            IEnumerable<string> removedTeachers = coursTeachersId.Except(course.TeachersId);
            dbCourse.Name = course.Name;
            dbCourse.IsActive = course.IsActive;
            dbCourse.Price = course.Price;
            var state = _context.Courses.Update(dbCourse);
            if (state.State != EntityState.Modified)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не оновлений" });
            }
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
                    {
                        await _userManager.AddToRoleAsync(user, Common.UsersConstants.teacher);
                    }
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
                        {
                            await _userManager.RemoveFromRoleAsync(user, Common.UsersConstants.teacher);
                        }
                    }
                }
            }
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteCourse(string Id)
        {
            Course course = _context.Courses
                .Include(sub => sub.Subscriptions)
                .Include(teach => teach.CourseAppUsers)
                .FirstOrDefault(co => co.Id == Id);
            if (course.Subscriptions.Count() == 0)
            {
                course.CourseAppUsers = null;
                _context.SaveChanges();
                var state = _context.Courses.Remove(course);
                if (state.State == EntityState.Deleted)
                {
                    await _context.SaveChangesAsync();
                    return IdentityResult.Success;
                }
                return IdentityResult.Failed(new IdentityError { Description = "Курс не видалений" });
            }
            return IdentityResult.Failed(new IdentityError { Description = "Курс не видалений, курс має абонементи" });
        }

        public async Task<bool> CourseHasSubscriptions(string id)
        {
            if ((await _context.Courses
                .Include(cour => cour.Subscriptions)
                .FirstOrDefaultAsync(cour => cour.Id == id))
                .Subscriptions.Count() > 0)
            {
                return true;
            }
            return false;
        }

        //Subscriptions
        public IEnumerable<Subscription> GetAllSubscriptions()
        {
            return _context.Subscriptions.Include(cour => cour.Course)
                                        .Include(payed => payed.PayedTo)
                                        .Include(user => user.AppUser)
                                        .ToList();
        }

        public async Task RemoveSubscriptionAsync(string id)
        {
            Subscription subscription = await _context.Subscriptions
                                                .Include(pay => pay.PayedTo)
                                                .FirstOrDefaultAsync(subs => subs.Id == id);
            if (subscription != null)
            {
                _context.Subscriptions.Remove(subscription);
                await _context.SaveChangesAsync();
            }
            else
            {
                //Not found Exception
            }
        }

        public async Task<Subscription> GetSubscription(string id)
        {
            Subscription subscription = await _context.Subscriptions
                            .Include(usr => usr.AppUser)
                            .Include(pay => pay.PayedTo)
                            .Include(cour => cour.Course)
                            .FirstOrDefaultAsync(subs => subs.Id == id);
            return subscription;
        }



        //School

        public IEnumerable<School> GetAllSchools()
        {
            return _context.Schools.Include(usr => usr.Owner).ToList();
        }

        public async Task<IdentityResult> CreateSchoolAsync(School school)
        {
            if (string.IsNullOrEmpty(school.Name))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Введіть назву школи" });
            }
            if (string.IsNullOrEmpty(school.UrlName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Введіть Url школи" });
            }
            if (string.IsNullOrEmpty(school.OwnerId))
            {

            }
            AppUser newOwner = await _userManager.FindByIdAsync(school.OwnerId);
            if (newOwner == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Додайте диретора" });
            }
            var state = await _context.Schools.AddAsync(school);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа не додана" });
            }
            await _context.SaveChangesAsync();
            if (!(await _userManager.IsInRoleAsync(newOwner, Common.UsersConstants.schoolOwner)))
            {
                await _userManager.AddToRoleAsync(newOwner, Common.UsersConstants.schoolOwner);
            }
            return IdentityResult.Success;
        }

        public async Task<School> GetSchoolAsync(string id)
        {
            return await _context.Schools
                .Include(sch => sch.Courses)
                .Include(sch => sch.Owner)
                .FirstOrDefaultAsync(sch => sch.Id == id);
        }

        public async Task<IdentityResult> UpdateSchoolAsync(School school)
        {
            School dbSchool = await _context.Schools.FirstOrDefaultAsync(sch => sch.Id == school.Id);
            if (dbSchool == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа на знайдена" });
            }
            if (string.IsNullOrEmpty(school.Name))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Введіть назву школи" });
            }
            if (string.IsNullOrEmpty(school.UrlName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Введіть Url школи" });
            }
            dbSchool.Name = school.Name;
            dbSchool.OwnerId = school.OwnerId;
            dbSchool.UrlName = school.UrlName;
            var state = _context.Schools.Update(dbSchool);
            if (state.State != EntityState.Modified)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа не оновлена" });
            }
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteScoolAsync(string Id)
        {
            School dbSchool = await _context.Schools.FirstOrDefaultAsync(sch => sch.Id == Id);
            if (dbSchool == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа на знайдена" });
            }
            var state = _context.Schools.Remove(dbSchool);
            if (state.State != EntityState.Deleted)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа не видалена" });
            }
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ChengeOwnerAsync(string newOwnerId, string schoolId)
        {
            if (string.IsNullOrEmpty(newOwnerId) || string.IsNullOrEmpty(schoolId))
            {
                return IdentityResult.Failed();
            }
            AppUser newOwner = await _userManager.FindByIdAsync(newOwnerId);
            if (newOwner == null)
            {
                return IdentityResult.Failed();
            }
            School school = await _context.Schools.FirstOrDefaultAsync(scg => scg.Id == schoolId);
            if (school == null)
            {
                return IdentityResult.Failed();
                ;
            }
            if (school.OwnerId != null
                && _context.Schools.Where(sch => sch.OwnerId == school.OwnerId).ToList().Count() == 1)
            {
                AppUser oldOwner = await _userManager.FindByIdAsync(school.OwnerId);
                if (oldOwner != null && (await _userManager.IsInRoleAsync(oldOwner, Common.UsersConstants.schoolOwner)))
                {
                    await _userManager.RemoveFromRoleAsync(oldOwner, Common.UsersConstants.schoolOwner);
                }
            }
            if (!(await _userManager.IsInRoleAsync(newOwner, Common.UsersConstants.schoolOwner)))
            {
                await _userManager.AddToRoleAsync(newOwner, Common.UsersConstants.schoolOwner);
            }
            school.OwnerId = newOwner.Id;
            _context.Schools.Update(school);
            await _context.SaveChangesAsync();
            return IdentityResult.Success;
        }


    }
}
