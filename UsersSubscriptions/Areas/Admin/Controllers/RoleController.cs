using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.DomainServices;
using UsersSubscriptions.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using UsersSubscriptions.Common;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UsersConstants.admin)]
    public class RoleController : Controller
    {
        private IRoleService _roleService;
        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public IActionResult Index()
        {
            return View(_roleService.GetAllRoles());
        }


        public IActionResult CreateRole() => View();

        [HttpPost]
        public async Task<IActionResult> CreateRole(IdentityRole model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                await _roleService.CreateRoleAsync(model);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RoleDetails(string id)
        {
            IdentityRole role = await _roleService.GetRoleAsync(id);
            if (role != null)
            {
                ViewBag.roleUsers = await _roleService.GetRoleUsersAsync(role.Name);
                return View(role);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                await _roleService.UpdateRole(model);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRole(IdentityRole model)
        {
            await _roleService.DeleteRoleAsync(model.Id);
            return RedirectToAction(nameof(Index));
        }

    }
}