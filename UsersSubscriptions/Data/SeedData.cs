﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UsersSubscriptions.Models;
using UsersSubscriptions.Common;
using UsersSubscriptions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace UsersSubscriptions.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            ApplicationDbContext _context = new ApplicationDbContext(serviceProvider.GetRequiredService<
                    DbContextOptions<ApplicationDbContext>>());
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
            await CreateRolesAsync(roleManager);
            await CreateUsersAsync(userManager);
            await CreateSchoolAsync(_context, userManager);
            //await CreateSubscriptionsAsync(_context, userManager);
        }

        public static async Task CreateRolesAsync(RoleManager<IdentityRole> _roleManager)
        {
            List<string> roles = new List<string>
                { UsersConstants.admin, UsersConstants.teacher, UsersConstants.user, UsersConstants.schoolOwner};

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
            List<AppUser> appUsers = new List<AppUser>
            {
                new AppUser
                {
                    UserName="yuriy.kataytsev@gmail.com",
                    Email="yuriy.kataytsev@gmail.com",
                    FullName="Юрий Катайцев",
                    PhoneNumber="0950413008",
                    IsActive=true,
                },
                new AppUser
                {
                    UserName="faraon.ua@gmail.com",
                    Email="faraon.ua@gmail.com",
                    FullName="Дубровський Олександр",
                    PhoneNumber="0630517004",
                    IsActive=true,
                },
                new AppUser
                {
                    UserName="kataytseva.irina@gmail.com",
                    Email="kataytseva.irina@gmail.com",
                    FullName="Катайцева Ирина",
                    PhoneNumber="0950413008",
                    IsActive=true,
                },
                new AppUser
                {
                    UserName="ivanov.petr@mail.com",
                    Email="ivanov.petr@mail.com",
                    FullName="Іванов",
                    PhoneNumber="09500000001",
                    IsActive=true,
                },new AppUser
                {
                    UserName="moroz@mail.com",
                    Email="moroz@mail.com",
                    FullName="Иван Мороз",
                    PhoneNumber="09500000002",
                    IsActive=true,
                },new AppUser
                {
                    UserName="tester@mail.com",
                    Email="tester@gmail.com",
                    FullName="Зайцева Ирина",
                    PhoneNumber="0950000003",
                    IsActive=true,
                },

            };
            foreach (AppUser user in appUsers)
            {
                if (await _userManager.FindByNameAsync(user.UserName) == null)
                {
                    await _userManager.CreateAsync(user, password);
                    await AddRolesToUsers(_userManager, user.UserName);
                }
            }
        }

        private static async Task AddRolesToUsers(UserManager<AppUser> _userManager, string userName)
        {
            AppUser user = await _userManager.FindByNameAsync(userName);
            if (user != null)
            {
                await _userManager.AddToRoleAsync(user, UsersConstants.user);
                if (userName.ToLower().Contains("faraon.ua") || userName.ToLower().Contains("yuriy.kataytsev"))
                {
                    await _userManager.AddToRoleAsync(user, UsersConstants.teacher);
                    await _userManager.AddToRoleAsync(user, UsersConstants.admin);
                    await _userManager.AddToRoleAsync(user, UsersConstants.schoolOwner);
                }


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
                    //await _userManager.AddToRoleAsync(user, UsersConstants.admin);
                }
            }
        }

        public static async Task CreateSchoolAsync(ApplicationDbContext _context, UserManager<AppUser> _userManager)
        {
            string schoolName = "Best dance school";
            School school = _context.Schools.FirstOrDefault(sch => sch.Name == schoolName);
            if (school == null)
            {
                AppUser user = await _userManager.FindByNameAsync("faraon.ua@gmail.com");
                School newSchool = new School
                {
                    Name = schoolName,
                    OwnerId = user.Id,
                };
                await _context.Schools.AddAsync(newSchool);
                await _context.SaveChangesAsync();
                school = _context.Schools.FirstOrDefault(sch => sch.Name == schoolName);
                await CreateCoursesAsync(_context, _userManager, school.Id);
            }

        }

        static async Task CreateCoursesAsync(ApplicationDbContext _context,
                                                    UserManager<AppUser> _userManager,
                                                    string schoolId)
        {
            List<Course> courses = new List<Course>
            {
                new Course
                {
                    Name="Bachata",
                    IsActive=true,
                    Description="Dancing",
                    Price=800,
                    SchoolId=schoolId,
                },
                new Course
                {
                    Name="Samba",
                    IsActive=true,
                    Description="Dancing",
                    Price=500,
                     SchoolId=schoolId,
               },
                new Course
                {
                    Name="Street Dancing",
                    IsActive=true,
                    Description="Dancing",
                    SchoolId=schoolId,
                    Price=400,
                },
            };

            List<AppUser> teachers = new List<AppUser> {
                await _userManager.FindByNameAsync("yuriy.kataytsev@gmail.com"),
                await _userManager.FindByNameAsync("faraon.ua@gmail.com"),
            };
            foreach (Course curCours in courses)
            {
                Course dbCourse = await _context.Courses
                     .FirstOrDefaultAsync(cour => cour.Name == curCours.Name && cour.SchoolId == curCours.SchoolId);
                if (dbCourse == null)
                {
                    await _context.Courses.AddAsync(curCours);
                    await _context.SaveChangesAsync();
                    dbCourse = await _context.Courses
                     .FirstOrDefaultAsync(cour => cour.Name == curCours.Name && cour.SchoolId == curCours.SchoolId);
                    IList<CourseAppUser> courseAppUsers = teachers.Select(teach => new CourseAppUser
                    {
                        AppUserId = teach.Id,
                        CourseId = dbCourse.Id,
                    }).ToList();
                    dbCourse.CourseAppUsers = courseAppUsers;
                    _context.Courses.Update(dbCourse);
                    await _context.SaveChangesAsync();
                    await CreateSubscriptionsAsync(_context, _userManager, dbCourse);
                }
            }
        }


        static async Task CreateSubscriptionsAsync(ApplicationDbContext _context,
                                                           UserManager<AppUser> _userManager,
                                                           Course dbCourse)
        {
            List<string> studentNames = new List<string>
            { "kataytseva.irina@gmail.com","ivanov.petr@mail.com","moroz@mail.com","tester@mail.com" };
            string teacherId = dbCourse.CourseAppUsers.FirstOrDefault().AppUserId;
            List<AppUser> students = new List<AppUser>();
            foreach (string studentName in studentNames)
            {
                AppUser student = await _userManager.FindByNameAsync(studentName);
                if (student != null)
                {
                    students.Append(student);
                    Subscription subscription = new Subscription
                    {
                        AppUserId = student.Id,
                        Month = DateTime.Now,
                        CourseId = dbCourse.Id,
                        PayedToId = teacherId,
                        PayedDatetime = DateTime.Now,
                        Price = dbCourse.Price,
                    };
                    await _context.Subscriptions.AddAsync(subscription);
                    Subscription prewSubscription = new Subscription
                    {
                        AppUserId = student.Id,
                        Month = DateTime.Now.AddMonths(1),
                        CourseId = dbCourse.Id,
                        PayedToId = teacherId,
                        PayedDatetime = DateTime.Now,
                        Price = dbCourse.Price,
                    };
                    await _context.Subscriptions.AddAsync(prewSubscription);
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}
