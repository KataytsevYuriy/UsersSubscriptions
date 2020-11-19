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
using UsersSubscriptions.DomainServices;
using UsersSubscriptions.Models;

namespace UsersSubscriptions.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = UsersConstants.admin)]
    public class FinanceController : Controller
    {
        private ISchoolService _schoolService;
        private IPaymentService _paymentService;
        public FinanceController(ISchoolService schoolService, IPaymentService paymentService)
        {
            _schoolService = schoolService;
            _paymentService = paymentService;
        }

        public IActionResult Index()
        {
            return View(_schoolService.GetAllSchools());
        }

        public IActionResult SchoolDetails(string schoolId)
        {
            if (string.IsNullOrEmpty(schoolId)) return BadRequest();
            School school = _paymentService.GetSchoolFinance(schoolId);
            if (school == null) return NotFound();
            return View(school);
        }

        [HttpPost]
        public IActionResult SchoolDetails(School school)
        {
            if (school == null) return NotFound();
            IdentityResult result = _paymentService.UpdateSchoolFinance(school);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Школа оновлена";
                school = _schoolService.GetSchool(school.Id);
            }
            else
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return View(_paymentService.GetSchoolFinance(school.Id));
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
            School school = _paymentService.GetSchoolFinance(transaction.SchoolId);
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
            IdentityResult result = _paymentService.AddSchoolTransaction(newTransaction);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Баланс школи успішно поповнено";
                school = _schoolService.GetSchool(school.Id);
            }
            else
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return RedirectToAction("SchoolDetails", new { schoolId = transaction.SchoolId });
        }

        [HttpPost]
        public IActionResult RemoveLastSchoolTransaction(School school)
        {
            IdentityResult result = _paymentService.RemoveLastSchoolTransaction(school.Id);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Транзакцію успішно видалено";
            } else
            {
                TempData["ErrorMessage"] = result.Errors.FirstOrDefault().Description;
            }
            return RedirectToAction(nameof(SchoolDetails), new { schoolId = school.Id });
        }
    }
}