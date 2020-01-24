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
                UserDetailViewModel model = new UserDetailViewModel()
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    AllRoles = allRoles,
                    UserRoles = userRoles
                };
                return  View(model);
            }
            return View(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> UserDetails(UserDetailViewModel model)
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
                    //user.IsActive = model.IsActive;
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
        // GET: Admin/Create
        public ActionResult Create()
        {
            return View();
        }

        // Roles
        public IActionResult Roles()
        {
            //IdentityRole roleAdmin = new IdentityRole { Name = "Teacher", NormalizedName = "Teacher" };
            //await _context.Roles.AddAsync(roleAdmin);
            //await _context.SaveChangesAsync();

            //IEnumerable<IdentityRole> roles = await _context.Roles;
            //_userManager.rol
            //IEnumerable<IdentityRole> roles = _context.Roles as IEnumerable<IdentityRole>;
            //List<IdentityRole> roles = _context.Roles.ToList();
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }
        // POST: Admin/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add insert logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Admin/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: Admin/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: Admin/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: Admin/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}