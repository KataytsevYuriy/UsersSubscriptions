﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UsersSubscriptions.Models;
using UsersSubscriptions.Services;

namespace UsersSubscriptions.Controllers
{
    [Authorize(Roles =Common.UsersConstants.user)]
    public class StudentController : Controller
    {
        private IUserRepository repository;
        public StudentController(IUserRepository repo) => repository = repo;

        public async Task<IActionResult> Index()
        {
            AppUser currentUser = await repository.GetCurentUser(HttpContext);
            if (currentUser != null)
            {
                Byte[] imgBytes = Qrcoder.GenerateQRCode(currentUser.Id);
                ViewBag.Image = imgBytes;
                Qrcoder.CreateQrFile(currentUser.Id);
                Byte[] qrFromFile = Qrcoder.GetQrFile(currentUser.Id);
                ViewBag.LoadedQrFile = qrFromFile;
            }
           return View(currentUser);
        }

        public async Task<IActionResult> AddToCourse(string id)
        {
            ViewBag.course = await repository.GetCourse(id);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddToCourse(Subscription subscription)
        {
            AppUser currentUser = await repository.GetCurentUser(HttpContext);
            subscription.AppUser = currentUser;
            subscription.AppUserId = currentUser.Id;
            subscription.CreatedDatetime = DateTime.Now;
            IdentityResult result = await repository.CreateSubscription(subscription);
            if (result.Succeeded)
            {
            TempData["Result"] = result.Succeeded;
            }
            if (result.Errors.Count() > 0)
            {
                string erorMessage = "";
                foreach (var error in result.Errors)
                {
                    erorMessage += error.Description;
                }
                TempData["ErorMessage"] = erorMessage;
            }
            return RedirectToAction(nameof(UserCourses));
        }

        public async Task<IActionResult> UserCourses()
        {
            AppUser currentUser = await repository.GetCurentUser(HttpContext);
            AppUser appUser = repository.GetUserCourses(currentUser.Id);
            //IEnumerable<Subscription> subscriptions = appUser.Subscriptions.OrderBy(ord => ord.Id);
            //IEnumerable<Subscription> subscriptions1 = appUser.Subscriptions.OrderBy(ord => ord.CreatedDatetime);
            //appUser.Subscriptions = appUser.Subscriptions.OrderBy(cours => cours.DayStart);
            
                ViewBag.Result = TempData["Result"];
                ViewBag.ErorMessage = TempData["ErorMessage"];

            return View(appUser);
        }

        public IActionResult AllCourses()
        {
            return View(repository.GetAllCourses());
        }

        public async Task<IActionResult> RemoveSubscription(string Id)
        {
            Subscription subscription = await repository.GetSubscriptionAsync(Id);
            return View(subscription);
        }

        [HttpPost]
        public async Task<IActionResult> RemoveSubscription (Subscription subscription)
        {
            await repository.DeleteSubscription(subscription.Id);
            return RedirectToAction(nameof(UserCourses));
        }
    }
}