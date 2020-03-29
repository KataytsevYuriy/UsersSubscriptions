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

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = UsersConstants.admin)]
    public class UserController : Controller
    {
        private IAdminDataRepository repository;
        public UserController(IAdminDataRepository aRepo)
        {
            repository = aRepo;
         }
        public ActionResult Index()
        {
            return View(repository.GetAllUsers());
        }

        public async Task<ActionResult> UserDetails(string id)
        {
            AppUser user = await repository.GetUserAsync(id);
            if (user != null)
            {
                var userRoles = await repository.GetUserRolesAsync(user.Id);
                var allRoles = repository.GetAllRoles();
                UserViewModel model = new UserViewModel()
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive? "checked=\"checked\"" : "",
                    AllRoles = allRoles,
                    UserRoles = userRoles
                };
                return  View(model);
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
                                                        Email=model.Email,
                                                        IsActive = model.IsActive == null ? false : true
                                                    }, model.UserRoles);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DeleteUser(string id)
        {
            AppUser user = await repository.GetUserAsync(id);
            if (user != null)
            {
                ViewBag.userRoles = await repository.GetUserRolesAsync(user.Id);
                return View(user);
            }
            return View(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(AppUser model)
        {
            await repository.DeleteUseAsyncr(model.Id);
            return RedirectToAction(nameof(Index));
        }
 
    }
}