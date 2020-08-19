using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    [Authorize(Roles =UsersConstants.admin)]
    public class FinanceController : Controller
    {
        private IAdminDataRepository repository;
        private ITeacherRepository repositoryTeacher;
        public FinanceController(IAdminDataRepository repo, ITeacherRepository repoTeacher)
        {
            repository = repo;
            repositoryTeacher = repoTeacher;
        }

        public IActionResult Index()
        {
            return View(repository.GetAllSchools());
        }

        public IActionResult SchoolDetails(string schoolId)
        {
            if (string.IsNullOrEmpty(schoolId)) return BadRequest();
            School school = repository.GetSchoolFinance(schoolId);
            if (school == null) return NotFound();
            return View(school);
        }

        [HttpPost]
        public IActionResult SchoolDetails(School school)
        {
            if (school == null) return NotFound();
            IdentityResult result = repository.UpdateSchoolFinance(school);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа оновлена";
                school = repositoryTeacher.GetSchool(school.Id);
            }
            else
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return View(repository.GetSchoolFinance(school.Id));
        }

        [HttpPost]
        public IActionResult AddSchoolPayment(SchoolTransaction transaction)
        {
            if (string.IsNullOrEmpty(transaction.SchoolId)) return NoContent();
            if (transaction.Payed == 0)
            {
                TempData["ErrorMessage"] = "Сума повинна бути відмінна від 0";
                return RedirectToAction("SchoolDetails", new { schoolId = transaction.SchoolId });
            }
            School school = repository.GetSchoolFinance(transaction.SchoolId);
            if (school == null) return NotFound();
            SchoolTransaction newTransaction = new SchoolTransaction
            {
                Payed = transaction.Payed,
                OldBalance = school.Balance,
                NewBalance = school.Balance + transaction.Payed,
                Description = transaction.Description,
                PayedDateTime = DateTime.Now,
                SchoolId = transaction.SchoolId,
            };
            IdentityResult result = repository.AddSchoolTransaction(newTransaction);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Баланс школи успішно поповнено";
                school = repositoryTeacher.GetSchool(school.Id);
            }
            else
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return RedirectToAction("SchoolDetails",new { schoolId = transaction.SchoolId });
        }
    }
}