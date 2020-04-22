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

        //public async Task<IEnumerable<AppUser>> GetTeachersInCourse(string courseId)
        // {
        //     var teachersId = _context.Set<CourseAppUser>.
        //     return await _context.Users.Include(cour => cour.CourseAppUsers).ThenInclude(usr => usr.Course).Where(c=>c.CourseAppUsers.each)
        //         Select(p => p.CourseAppUsers.Appuser);
        // }


        public async Task UpdateUserAsync(AppUser user, IList<string> newUserRoles)
        {
            AppUser dbUser = await _userManager.FindByIdAsync(user.Id);
            if (dbUser != null)
            {
                dbUser.FullName = user.FullName;
                dbUser.UserName = user.UserName;
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
            //AppUser user = await GetUserAsync(id);
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

        public async Task CreateCourseAsync(CourseViewModel model)
        {
            Course course = await _context.Courses.FindAsync(model.Name);
            if (course == null)
            {
                course = new Course();
                course.Name = model.Name;
                course.IsActive = model.IsActive;
                course.Price = model.Price;
                await _context.Courses.AddAsync(course);
                await _context.SaveChangesAsync();
                Course newCourse = await _context.Courses
                    .FirstOrDefaultAsync(cour => cour.Name.Equals(model.Name));
                newCourse.CourseAppUsers = model.NewTeachers.Select(teachId => new CourseAppUser
                {
                    AppUserId = teachId,
                    CourseId = newCourse.Id
                }).ToList();
                _context.Courses.Update(newCourse);
                await _context.SaveChangesAsync();
            }
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
            dbCourse.Name = course.Name;
            dbCourse.IsActive = course.IsActive;
            dbCourse.Price = course.Price;
            dbCourse.CourseAppUsers = course.TeachersId.Select(tech => new CourseAppUser
            {
                AppUserId = tech,
                CourseId = course.Id,
            }).ToList();
            var state = _context.Courses.Update(dbCourse);
            if (state.State != EntityState.Modified)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Курс не оновлений" });
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
            var state = await _context.Schools.AddAsync(school);
            if (state.State != EntityState.Added)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа не додана" });
            }
            await _context.SaveChangesAsync();
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


    }
}
