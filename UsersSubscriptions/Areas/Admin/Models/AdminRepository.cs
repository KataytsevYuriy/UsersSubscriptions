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

        public async Task<IdentityResult> DeleteUseAsync(string id)
        {
            IdentityResult result = new IdentityResult();
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return (IdentityResult.Failed(new IdentityError { Description = "Користувача не видалено." }));
            }
            IEnumerable<CourseAppUser> courseAppUsers = _context.CourseAppUsers.Where(cap => cap.AppUserId == id);
            if (courseAppUsers.Count() > 0)
            {
                _context.CourseAppUsers.RemoveRange(courseAppUsers);
            }
            IEnumerable<Subscription> subscriptions = _context.Subscriptions.Where(sub => sub.AppUserId == id);
            if (subscriptions.Count() > 0)
            {
                _context.Subscriptions.RemoveRange(subscriptions);
            }
            IEnumerable<School> schools = _context.Schools.Where(sch => sch.OwnerId == id);
            if (schools.Count() > 0)
            {
                string message = "";
                foreach (School school in schools) message += school.Name + ", ";
                return (IdentityResult.Failed(new IdentityError
                {
                    Description = "Користувач це власник школи - "
                    + message + " Спочатку видаліть школи"
                }));
            }
            IList<string> roles = await GetUserRolesAsync(id);
            if (roles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, roles);
            }
            result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _context.SaveChanges();
            }
            else
            {
                result = IdentityResult.Failed(new IdentityError { Description = "Роль не видалена." });
            }
            return (result);

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

        public async Task UpdateRole(IdentityRole model)
        {
            IdentityRole role = await GetRoleAsync(model.Id);
            if (role != null)
            {
                role.Name = model.Name;
                await _roleManager.UpdateAsync(role);
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

        public async Task DeleteRoleAsync(string id)
        {
            IdentityRole role = await GetRoleAsync(id);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }
        }


        //Subscriptions
        public IEnumerable<Subscription> GetFilteredSubscriptions(string schoolId, string courseId, DateTime month, string searchByName)
        {
            IEnumerable<Subscription> subscriptions = _context.Subscriptions
                                        .Include(cour => cour.Course).ThenInclude(cou => cou.School)
                                        .Include(payed => payed.PayedTo)
                                        .Include(user => user.AppUser)
                                        .ToList();
            if (!string.IsNullOrEmpty(schoolId))
            {
                subscriptions = subscriptions.Where(sub => sub.Course.SchoolId == schoolId);
            }
            if (!string.IsNullOrEmpty(courseId))
            {
                subscriptions = subscriptions.Where(sub => sub.CourseId == courseId);
            }
            if (month.Year > 2010)
            {
                subscriptions = subscriptions.Where(sub => sub.Month.Year == month.Year
                  && sub.Month.Month == month.Month);
            }
            if (!string.IsNullOrEmpty(searchByName))
            {
                subscriptions = subscriptions
                    .Where(sub =>
                    (sub.AppUser!=null && sub.AppUser.FullName.ToLower().Contains(searchByName.ToLower()))
                    ||(sub.AppUser==null && !string.IsNullOrEmpty(sub.FullName) && sub.FullName.ToLower().Contains(searchByName.ToLower())));

            }
            return subscriptions;
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
            return _context.Schools
                .Include(usr => usr.Owner)
                .Include(sch => sch.Courses)
                .ToList();
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
                return IdentityResult.Failed(new IdentityError { Description = "Додайте власника  школи" });
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
            string oldOwnerId = dbSchool.OwnerId ?? "";
            if (string.IsNullOrEmpty(school.OwnerId))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Додайте власника  школи" });
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
            if (!oldOwnerId.Equals(school.OwnerId))
            {
                AppUser newOwner = await _userManager.FindByIdAsync(school.OwnerId);
                if (newOwner != null && !(await _userManager.IsInRoleAsync(newOwner, Common.UsersConstants.schoolOwner)))
                {
                    await _userManager.AddToRoleAsync(newOwner, Common.UsersConstants.schoolOwner);
                }
                if (_context.Schools
                    .Include(sch => sch.Owner)
                    .Where(sch => sch.OwnerId == oldOwnerId)
                    .Count() == 1)
                {
                    AppUser oldOwner = await _userManager.FindByIdAsync(oldOwnerId);
                    if (oldOwner != null)
                    {
                        await _userManager.RemoveFromRoleAsync(oldOwner, Common.UsersConstants.schoolOwner);
                    }
                }
            }
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteScoolAsync(string Id)
        {
            School dbSchool = await _context.Schools
                .Include(sch => sch.Courses)
                .FirstOrDefaultAsync(sch => sch.Id == Id);
            if (dbSchool == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа на знайдена" });
            }
            if (dbSchool.Courses.Count() > 0)
            {
                return IdentityResult.Failed(new IdentityError { Description = "Школа має курси, видаліть їх спочатку." });
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
            }
            if (school.OwnerId != null
                && _context.Schools.Where(sch => sch.OwnerId == school.OwnerId).ToList().Count() == 1)
            {
                AppUser oldOwner = await _userManager.FindByIdAsync(school.OwnerId);
                if (oldOwner != null && (_context.Schools
                    .Include(sch => sch.Owner)
                    .Where(sch => sch.OwnerId == oldOwner.Id)
                    .Count() == 1))
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
