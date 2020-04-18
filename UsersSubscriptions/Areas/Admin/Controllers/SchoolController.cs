using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Areas.Admin.Models;
using UsersSubscriptions.Common;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UsersConstants.admin)]
    public class SchoolController : Controller
    {
        private IAdminDataRepository repository;
        public SchoolController(IAdminDataRepository repo)
        {
            repository = repo;
        }

        public IActionResult Index()
        {
            return View(repository.GetAllSchools());
        }

        public async Task<IActionResult> CreateSchool()
        {
            ViewBag.owners = await repository.GetRoleUsersAsync(UsersConstants.schoolOwner);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchool(School school)
        {
            IdentityResult result = await repository.CreateSchoolAsync(school);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа додана";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(CreateSchool));
        }

        public async Task<IActionResult> SchoolDetails(string id)
        {
            School school = await repository.GetSchoolAsync(id);
            if (school == null)
            {
                TempData["ErrorMessage"] = "Школа не знайдена";
                return RedirectToAction(nameof(Index));
            }
            return View(school);
        }
    }
}