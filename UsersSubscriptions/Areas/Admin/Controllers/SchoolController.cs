﻿using System;
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

        [HttpPost]
        public async Task<IActionResult> SchoolDetails(School school)
        {
            IdentityResult result = await repository.UpdateSchoolAsync(school);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа оновлена";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(SchoolDetails), new { id = school.Id });
        }

        public async Task<IActionResult> DeleteSchool (string id)
        {
            School school = await repository.GetSchoolAsync(id);
            if (school == null)
            {
                TempData["ErrorMessage"] = "Школа не знайдена";
                return RedirectToAction(nameof(Index));
            }
            return View(school);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSchool(School school)
        {
            IdentityResult result = await repository.DeleteScoolAsync(school.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа видалена";
                return RedirectToAction(nameof(Index));
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(Index));
        }

        public IActionResult CreateCourse(string id)
        {
            return View(new Course { SchoolId = id });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(Course course)
        {
            IdentityResult result = await repository.CreateCourseAsync(course);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс доданий";
                return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return View(course);
        }

        public async Task<IActionResult> CourseDetails(string id)
        {
            Course course = await repository.GetCourseAsync(id);
            CourseDetailsViewModel model = new CourseDetailsViewModel
            {
                Id = course.Id,
                Name = course.Name,
                IsActive = course.IsActive,
                Price = course.Price,
                CourseAppUsers = course.CourseAppUsers,
                SchoolId = course.SchoolId,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCourse(CourseDetailsViewModel course)
        {
            IdentityResult result = await repository.UpdateCourseAsync(course);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс оновлений";
                return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(CourseDetails), new { id = course.Id });
        }

        public async Task<IActionResult> DeleteCourse(string id)
        {
            ViewBag.HasSubscriptions = await repository.CourseHasSubscriptions(id);
            return View(await repository.GetCourseAsync(id));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(Course course)
        {
            var result = await repository.DeleteCourse(course.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Курс видалений";
                return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId });
            }
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            return RedirectToAction(nameof(SchoolDetails), new { id = course.SchoolId });
        }

        [HttpPost]
        public async Task<JsonResult> ChangeSchoolOwnerAsync(string id, string schoolId)
        { //  /ChangeSchoolOwnerAsync/" + userIdent.id + "?schoolId=" + courseId + "?oldOwner=" + oldOwner,

            if (id == null || string.IsNullOrEmpty(id)
                || schoolId == null || string.IsNullOrEmpty(schoolId))
            {
                return Json("");
            }
            if (await repository.GetUserAsync(id) == null)
            {
                return Json("");
            }
            if (await repository.GetSchoolAsync(schoolId) == null)
            {
                return Json("");
            }
            IdentityResult result = await repository.ChengeOwnerAsync(id, schoolId);
            if (!result.Succeeded)
            {
                return Json("");
            }
            return Json("true");
        }
    }
}