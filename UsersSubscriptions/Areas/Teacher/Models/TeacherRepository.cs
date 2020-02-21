using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using UsersSubscriptions.Models;
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

        public async Task<IEnumerable<Course>> GetTeacherCoursesAsync(AppUser teacher)
        {
            IEnumerable<Course> courses = await _context.Courses.Include(cu => cu.CourseAppUsers).ThenInclude(te=>te.AppUser)
                    .Where(cap => cap.CourseAppUsers.Any(dd => dd.AppUserId == teacher.Id)).ToListAsync();
            return courses;
        }
    }
}
