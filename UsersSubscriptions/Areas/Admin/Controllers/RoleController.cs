using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Areas.Admin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using UsersSubscriptions.Common;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = UsersConstants.admin)]
    public class RoleController : Controller
    {
        private IAdminDataRepository repository;
        public RoleController(IAdminDataRepository repo)
        {
            repository = repo;
        }

        public IActionResult Index()
        {
            return View(repository.GetAllRoles());
        }


        public IActionResult CreateRole() => View();

        [HttpPost]
        public async Task<IActionResult> CreateRole(IdentityRole model)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(model.Name))
            {
                await repository.CreateRoleAsync(model);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> RoleDetails(string id)
        {
            IdentityRole role = await repository.GetRoleAsync(id);
            if (role != null)
            {
                ViewBag.roleUsers = await repository.GetRoleUsersAsync(role.Name);
                return View(role);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(IdentityRole model)
        {
            if (ModelState.IsValid)
            {
                await repository.UpdateRole(model);
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteRole(string id)
        {
            IdentityRole role = await repository.GetRoleAsync(id);
            if (role != null)
            {
                ViewBag.roleUsers = await repository.GetRoleUsersAsync(role.Name);
                return View(role);
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> DeleteRole(IdentityRole model)
        {
            await repository.DeleteRoleAsync(model.Id);
            return RedirectToAction(nameof(Index));
        }
    }
}