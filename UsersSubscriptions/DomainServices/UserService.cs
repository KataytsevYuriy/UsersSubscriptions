using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UsersSubscriptions.Data;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.DomainServices
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ISubscriptionsService _subscriptionsService;
        private IRoleService _roleService;
        public UserService(ApplicationDbContext context, UserManager<AppUser> userManager,
            IRoleService roleService, ISubscriptionsService subscriptionsService)
        {
            _context = context;
            _userManager = userManager;
            _roleService = roleService;
            _subscriptionsService = subscriptionsService;
        }

        public async Task<AppUser> GetUserAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public AppUser GetUserByPhone(string phone)
        {
            return _context.Users.FirstOrDefault(us => Regex.Replace(us.PhoneNumber, @"[^\d]+", "") == (phone));
        }

        public IEnumerable<AppUser> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        public IEnumerable<AppUser> FindUserByName(string name)
        {
            name = name.ToLower();
            List<AppUser> appUsers = _context.Users.Where(user => user.FullName.ToLower().Contains(name)).OrderBy(usr => usr.FullName).ToList();
            return appUsers;
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
                IList<string> userRoles = await _roleService.GetUserRolesAsync(user.Id);
                IEnumerable<string> addedRoles = newUserRoles.Except(userRoles);
                var removedRoles = userRoles.Except(newUserRoles);
                await _userManager.AddToRolesAsync(dbUser, addedRoles);
                await _userManager.RemoveFromRolesAsync(dbUser, removedRoles);
            }
        }

        public async Task<IdentityResult> DeleteUserAsync(string id)
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
                foreach(Subscription subscription in subscriptions)
                {
                    _subscriptionsService.RemoveSubscription(subscription.Id);
                }
                //_context.Subscriptions.RemoveRange(subscriptions);
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
            IList<string> roles = await _roleService.GetUserRolesAsync(id);
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
    }
}
