using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;
using UsersSubscriptions.Common;
using UsersSubscriptions.Data;
using Microsoft.EntityFrameworkCore;

namespace UsersSubscriptions.Data
{
    public static class SeedData
    {
        public static async Task CreateRolesAsync(RoleManager<IdentityRole> _roleManager)
        {
            List<string> roles = new List<string>
                { UsersConstants.admin, UsersConstants.teacher, UsersConstants.user};

            foreach (string role in roles)
            {
                IdentityRole newRole = await _roleManager.FindByNameAsync(role);
                if (newRole == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = role });
                }
            }
        }

        public static async Task CreateUsersAsync(UserManager<AppUser> _userManager)
        {
            string password = "Kat-123";

            List<string> users = new List<string>
                    { "Admin", "Teacher", "Teacher1", "Tester", "Student", "Student1" };
            foreach (string user in users)
            {
                if (await _userManager.FindByNameAsync(user) == null)
                {
                    string newUserName = user + "@mail.com";
                    await _userManager.CreateAsync(new AppUser
                    {
                        FullName = user,
                        UserName = newUserName,
                        Email = user+"@mail.com",
                        IsActive=true,
                        PhoneNumber="0001234567",
                    }, password);
                    await AddRolesToUsers(_userManager, newUserName);
                }
            }
        }

        private static async Task AddRolesToUsers(UserManager<AppUser> _userManager, string userName)
        {
            AppUser user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, UsersConstants.user);
                if (userName.ToLower().Contains(UsersConstants.teacher.ToLower()))
                {
                    await _userManager.AddToRoleAsync(user, UsersConstants.teacher);
                }
                if (userName.ToLower().Contains(UsersConstants.admin.ToLower()))
                {
                    await _userManager.AddToRoleAsync(user, UsersConstants.teacher);
                    await _userManager.AddToRoleAsync(user, UsersConstants.admin);
                }
                if (userName.ToLower().Contains("tester"))
                {
                    await _userManager.AddToRoleAsync(user, UsersConstants.teacher);
                    await _userManager.AddToRoleAsync(user, UsersConstants.admin);
                }
            }
        }

        public static async Task CreateCoursesAsync(ApplicationDbContext _context,
                                                    UserManager<AppUser> _userManager)
        {
            List<Course> courses = new List<Course>
            {
                new Course
                {
                    Name="Bachata",
                    IsActive=true,
                    Description="Dancing",
                },
                new Course
                {
                    Name="Samba",
                    IsActive=true,
                    Description="Dancing",
                },
                new Course
                {
                    Name="Street Dancing",
                    IsActive=true,
                    Description="Dancing",
                },
            };
            List<string> teachers = new List<string> {"Teacher", "Teacher1", "Tester"};
            List<string> teachersId = new List<string>();
            foreach(string id in teachers)
            {
                AppUser user = await _userManager.FindByNameAsync(id + "@mail.com");
                if (user != null)
                {
                teachersId.Add(user.Id);
                }
            }
            foreach (Course newCourse in courses)
            {
                Course course = _context.Courses.FirstOrDefault(cour=>cour.Name==newCourse.Name);
                if (course == null)
                {
                    await _context.AddAsync(newCourse);
                    await _context.SaveChangesAsync();
                    Course curCourse = _context.Courses.Include(p => p.CourseAppUsers)
                            .FirstOrDefault(cour => cour.Name == newCourse.Name);
                    curCourse.CourseAppUsers = teachersId.Select(teachId => new CourseAppUser
                    {
                        AppUserId = teachId,
                        CourseId = curCourse.Id,
                    }).ToList();
                    await _context.SaveChangesAsync();
                }
            }
            
        }

        public static async Task CreateSubscriptionsAsync(ApplicationDbContext _context,
                                                            UserManager<AppUser> _userManager)
        {
            List<string> users = new List<string> {"Tester", "Student", "Student1" };
            List<AppUser> appUsers = new List<AppUser>();
            foreach(string userName in users)
            {
                AppUser appUser = await _userManager.FindByNameAsync(userName + "@mail.com");
                if (appUser != null)
                {
                    appUsers.Add(appUser);
                }
            }
            Course course = await _context.Courses.FirstAsync(cour => cour.Name == "Bachata");
            if (course != null && appUsers.Count>0)
            {
                foreach(AppUser user in appUsers)
                {
                    Subscription newSubscription = new Subscription
                    {
                        AppUser = user,
                        Course = course,
                        CreatedDatetime = DateTime.Now,
                        DayStart = DateTime.Now,
                    };
                    await _context.Subscriptions.AddAsync(newSubscription);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
