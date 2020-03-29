using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Principal;

namespace UsersSubscriptions.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public HomeController(ApplicationDbContext ctx,
                                UserManager<AppUser> manager,
                                RoleManager<IdentityRole> roleManager)
        {
            _userManager = manager;
            _roleManager = roleManager;
            _context = ctx;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Send(DateTime month)
        {
            DateTime dateTime = month;
            return RedirectToAction(nameof(Index));
        }
        //public IActionResult AdminCabinet()
        //{
        //   // IEnumerable<AppUser> res = context.Users as IEnumerable<AppUser>;
        //    return View(context.Users);
        //}
        //[Authorize(Policy = "onlyAdmin")]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public async Task<IActionResult> CreateRoles()
        {
            await SeedData.CreateRolesAsync(_roleManager);
            await SeedData.CreateUsersAsync(_userManager);
            await SeedData.CreateCoursesAsync(_context, _userManager);
            await SeedData.CreateSubscriptionsAsync(_context, _userManager);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Contact()
        {
            var IsAdminExist = await _roleManager.RoleExistsAsync(Common.UsersConstants.admin);
            var user = await _userManager.GetUserAsync(HttpContext.User);
            var roles =await _userManager.GetRolesAsync(user);
            var isInRole = await _userManager.IsInRoleAsync(user, Common.UsersConstants.admin);
            var ttt = HttpContext.User.IsInRole(Common.UsersConstants.admin);
            var usersInRole = await _userManager.GetUsersInRoleAsync(UsersSubscriptions.Common.UsersConstants.admin);
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
