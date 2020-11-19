using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Data;
using UsersSubscriptions.Areas.Admin.Models;
using UsersSubscriptions.Models;
using UsersSubscriptions.DomainServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using UsersSubscriptions.Common;
using UsersSubscriptions.Services;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UsersConstants.admin)]
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private IRoleService _roleService;
        public UserController(IRoleService roleService, IUserService userService)
        {
            _roleService = roleService;
            _userService = userService;
        }
        public ActionResult Index()
        {
            return View(_userService.GetAllUsers());
        }

        public async Task<ActionResult> UserDetails(string id)
        {
            AppUser user = await _userService.GetUserAsync(id);
            if (user != null)
            {
                var userRoles = await _roleService.GetUserRolesAsync(user.Id);
                var allRoles = _roleService.GetAllRoles();
                UserViewModel model = new UserViewModel()
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    AllRoles = allRoles,
                    UserRoles = userRoles
                };
                return View(model);
            }
            return View(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> UpdateUser(UserViewModel model)
        {
            await _userService.UpdateUserAsync(new AppUser
            {
                Id = model.Id,
                FullName = model.FullName,
                UserName = model.UserName,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email,
                IsActive = model.IsActive,
            }, model.UserRoles);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(AppUser model)
        {
            
            IdentityResult result = await _userService.DeleteUserAsync(model.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Корстувача видалено";
                FileServices remove = new FileServices();
                remove.Delete("wwwroot/qrr/file-" + model.Id + ".qrr");
                remove.Delete("wwwroot/avatars/avatar-" + model.Id + ".jpg");
            } else
            {
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return RedirectToAction(nameof(Index));
        }

    }
}