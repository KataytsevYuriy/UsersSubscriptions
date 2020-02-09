using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Data;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Models
{
    public interface IUserRepository
    {
        Task<AppUser> GetCurentUser(HttpContext context);
        IEnumerable<Course> GetAllCourses();
    }

    public class UserRepository :IUserRepository
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public UserRepository(ApplicationDbContext ctx,
                                UserManager<AppUser> manager,
                                RoleManager<IdentityRole> roleManager)
        {
            _userManager = manager;
            _roleManager = roleManager;
            _context = ctx;
        }
        
        public async Task<AppUser> GetCurentUser(HttpContext context)
        {
            AppUser user = await _userManager.GetUserAsync(context.User);
            return user;
        }

        public IEnumerable<Course> GetAllCourses()
        {
            return _context.Courses.Include(c => c.CourseAppUsers).ThenInclude(u=>u.AppUser).ToList();
        }

    }
}
