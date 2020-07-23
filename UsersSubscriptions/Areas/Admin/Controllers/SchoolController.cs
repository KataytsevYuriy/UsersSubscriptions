using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Areas.Admin.Models;
using UsersSubscriptions.Areas.Admin.Models.ViewModels;
using UsersSubscriptions.Common;
using UsersSubscriptions.Models;
using UsersSubscriptions.Models.ViewModels;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UsersConstants.admin)]
    public class SchoolController : Controller
    {
        private IAdminDataRepository repository;
        private ITeacherRepository repositoryTeacher;
        public SchoolController(IAdminDataRepository repo, ITeacherRepository repoTeacher)
        {
            repository = repo;
            repositoryTeacher = repoTeacher;
        }

        public IActionResult Index()
        {
            return View(repository.GetAllSchools());
        }

        public IActionResult CreateSchool()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchool(School school)
        {
            IdentityResult result = await repository.CreateSchoolAsync(school);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа додана";
                School dbSchool = repositoryTeacher.GetSchoolByUrl(school.UrlName);
                if (dbSchool != null)
                {
                    repositoryTeacher.AddDefaultPaymentTypesToSchool(dbSchool.Id);
                }
                return RedirectToRoute("default",
                    new { controller = "Owner", action = "SchoolDetails", id = dbSchool.Id, isItAdmin = true });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return View(school);
        }

        [HttpPost]
        public async Task<IActionResult> SchoolDetails(School school)
        {
            IdentityResult result = await repository.UpdateSchoolAsync(school);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа оновлена";
                school = repositoryTeacher.GetSchool(school.Id);
            }
            else
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            ViewBag.isItAdmin = true;
            return View("../Owner/SchoolDetails", school);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSchool(School school)
        {
            IdentityResult result = await repository.RemoveScoolAsync(school.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа видалена";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToRoute("default",
                new { controller = "Owner", action = "SchoolDetails", id = school.Id, isItAdmin = true });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeSchoolOwnerAsync(string id, string schoolId)
        { 
            if (id == null || string.IsNullOrEmpty(id)
                || schoolId == null || string.IsNullOrEmpty(schoolId))
            {
                return BadRequest();
            }
            if (await repositoryTeacher.GetUserAsync(id) == null)
            {
                return NotFound();
            }
            if ( repositoryTeacher.GetSchool(schoolId) == null)
            {
                return NotFound();
            }
            IdentityResult result = await repository.ChengeOwnerAsync(id, schoolId);
            if (!result.Succeeded)
            {
                return Conflict();
            }
            return Ok(result);
        }
    }
}