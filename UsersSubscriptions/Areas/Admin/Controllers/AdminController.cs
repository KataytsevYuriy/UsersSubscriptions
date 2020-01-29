using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Data;
using UsersSubscriptions.Areas.Admin.Models;
using UsersSubscriptions.Models;
using Microsoft.AspNetCore.Identity;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private UserManager<AppUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public AdminController(ApplicationDbContext context,
                                UserManager<AppUser> userManager,
                                RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        // GET: Admin
        public ActionResult Index()
        {
            IEnumerable<AppUser> users = _userManager.Users.ToList();
            return View(users);
        }

        // GET: Admin/Details/5
        public async Task<ActionResult> UserDetails(string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var allRoles = _roleManager.Roles.ToList();
                UserViewModel model = new UserViewModel()
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    //IsActive = $"IsActive:{user.IsActive}",
                    IsActive = user.IsActive? "checked=\"checked\"" : "",
                    AllRoles = allRoles,
                    UserRoles = userRoles
                };
                return  View(model);
            }
            return View(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> UserDetails(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await _userManager.FindByIdAsync(model.Id);
                if (user != null)
                {
                    user.FullName = model.FullName;
                    user.UserName = model.UserName;
                    user.Email = model.Email;
                    user.PhoneNumber = model.PhoneNumber;
                    user.IsActive = model.IsActive == null ? false : true;
                    var res = await _userManager.UpdateAsync(user);
                    var userRoles = await _userManager.GetRolesAsync(user);
                    var addedRoles = model.UserRoles.Except(userRoles);
                    var removedRoles = userRoles.Except(model.UserRoles);
                    await _userManager.AddToRolesAsync(user, addedRoles);
                    await _userManager.RemoveFromRolesAsync(user, removedRoles);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UserDeleting (string id)
        {
            AppUser user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                UserViewModel model = new UserViewModel()
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = $"{user.IsActive}",
                    UserRoles = userRoles
                };
                return View(model);
            }
            return View(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UserDeleting (UserViewModel model)
        {
            AppUser deletingUser = await _userManager.FindByIdAsync(model.Id);
            if (deletingUser != null)
            {
                var userRoles = await _userManager.GetRolesAsync(deletingUser);
                if (userRoles != null)
                {
                    await _userManager.RemoveFromRolesAsync(deletingUser, userRoles);
                }
                await _userManager.DeleteAsync(deletingUser);
            }
            return RedirectToAction(nameof(Index));
        }
       

        // Roles
        public IActionResult Roles()
        {
           
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }


        public IActionResult CreateRole() => View();

        [HttpPost]
        public async Task<IActionResult> CreateRole (IdentityRole model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                IdentityRole role = await _roleManager.FindByNameAsync(model.Name);
                if (role == null)
                {
                    role = new IdentityRole { Name = model.Name };
                    await _roleManager.CreateAsync(role);
                }
            }
            return RedirectToAction(nameof(Roles));
        }
        
        public async Task<IActionResult> RoleDetails(string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            RoleViewModel model = new RoleViewModel { Name = role.Name, Id=role.Id };
            if (role != null)
            {
                //IList<AppUser> users = await _userManager.GetUsersInRoleAsync(role.Name);
                var users = await _userManager.GetUsersInRoleAsync(role.Name);
                if (users.Count() != 0)
                {
                    model.users = users;
                }
                return View(model);
            }
            return RedirectToAction(nameof(Roles));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(RoleViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole role = await _roleManager.FindByIdAsync(model.Id);
                if (role != null && !(role.Name.Equals(model.Name)))
                {
                    role.Name = model.Name;
                    await _roleManager.UpdateAsync(role);
                }
            }
            return RedirectToAction(nameof(Roles));
        }

        public async Task<IActionResult> RoleDeleting (string id)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(id);
            if (role != null)
            {
                RoleViewModel model = new RoleViewModel { Id = role.Id, Name = role.Name };
                IList<AppUser> users = await _userManager.GetUsersInRoleAsync(role.Name);
                if (users.Count() != 0) model.users = users;
                return View(model);
            }
            return RedirectToAction(nameof(Roles));
        }
        [HttpPost]
        public async Task<IActionResult> RoleDeleting (RoleViewModel model)
        {
            IdentityRole role = await _roleManager.FindByIdAsync(model.Id);
            if (role != null)
            {
                await _roleManager.DeleteAsync(role);
            }
            return RedirectToAction(nameof(Roles));
        }

        //Courses
        public async Task<IActionResult> Courses()
        {
            //List<AppUser> user = new List<AppUser>{
            //   // await _userManager.FindByNameAsync("admin@mail.net"),
            //    await _userManager.FindByNameAsync("TEST@MAIL.COM"),

            //};
            //Course course = new Course { Name = "Test1", IsActive = true, Teachers = user };
            //await _context.Courses.AddAsync(course);
            //await _context.SaveChangesAsync();


            //List<Course> c = await _context.Courses.ToList();
            //await _context.Courses.Remove();


            //IEnumerable<CourseViewModel> model;
            //List<Course> courses = _context.Courses.ToList();
            //IList<AppUser> allTeachers = await _userManager.GetUsersInRoleAsync("Teacher");
            //ViewBag.AllTeachers = allTeachers;
            //if (courses.Count() > 0)
            //{
            //    foreach(Course course in courses)
            //    {
            //        var teachers = _context.Users.Where(u=>u.Id)
            //    }
            //}
            return RedirectToAction(nameof(Index));
        }

    }
}