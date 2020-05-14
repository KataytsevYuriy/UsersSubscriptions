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
        private IAdminDataRepository repository;
        private ITeacherRepository repositoryTeacher;
        public UserController(IAdminDataRepository repo, ITeacherRepository repoTeacher)
        {
            repository = repo;
            repositoryTeacher = repoTeacher;
        }
        public ActionResult Index()
        {
            return View(repository.GetAllUsers());
        }

        public async Task<ActionResult> UserDetails(string id)
        {
            AppUser user = await repositoryTeacher.GetUserAsync(id);
            if (user != null)
            {
                var userRoles = await repository.GetUserRolesAsync(user.Id);
                var allRoles = repository.GetAllRoles();
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
            await repository.UpdateUserAsync(new AppUser
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
            
            IdentityResult result = await repository.DeleteUseAsync(model.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Корстувача видалено";
                DeleteFile remove = new DeleteFile();
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