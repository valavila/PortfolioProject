﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DjSpot.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Hosting;
using DjSpot.Data;

namespace DjSpot.Controllers
{
    public class HomeController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly ApplicationDbContext DBcontext;

        public HomeController(ILogger<HomeController> logger, UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _logger = logger;
            DBcontext = context;
            webHostEnvironment = hostEnvironment;
        }
        public async Task<IActionResult> IndexAsync()
        {
            // Gets all registered users - to send to view
            var user = _userManager.Users;

            // Gets current user
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);

            // Send current user to get assigned as Dj or customer
            if (currentUser != null) 
            {
                await SetAsDjOrCustomer(currentUser);
            }

            return View(user);
        }

        /// <summary>
        /// Sets up current user field for isDj or isCustomer, depending on what option they chose when registering
        /// </summary>
        /// <param name="currentUser">Current logged in user</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> SetAsDjOrCustomer(ApplicationUser currentUser)
        {
            //ApplicationUser currentUser = await _userManager.GetUserAsync(User);
            if (ModelState.IsValid)
            {
                if (currentUser.UserType == userType.customer)
                {
                    currentUser.isCustomer = true;
                    currentUser.isDj = false;
                    await _userManager.AddToRoleAsync(currentUser, "Customer");

                }
                else if (currentUser.UserType == userType.dj)
                {
                    currentUser.isCustomer = false;
                    currentUser.isDj = true;
                    await _userManager.AddToRoleAsync(currentUser, "Dj");
                }
                
                

                DBcontext.Update(currentUser);
                await DBcontext.SaveChangesAsync();
            }
           
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Profile()
        {
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);

            ViewBag.UserName = currentUser.UserName;
            ViewBag.Email = currentUser.Email;
            ViewBag.Phone = currentUser.PhoneNumber;
            ViewBag.AboutMe = currentUser.Bio;
            ViewBag.Name = currentUser.FirstName + " " + currentUser.LastName;

            return View();
        }

        public async Task<IActionResult> DjPageAsync(string id)
        {
            ApplicationUser selectedDj = await _userManager.FindByIdAsync(id);

            ViewBag.DjName = selectedDj.UserName;
            ViewBag.DjEmail = selectedDj.Email;
            ViewBag.DJPhone = selectedDj.PhoneNumber;
            ViewBag.DjAboutMe = selectedDj.Bio;
            ViewBag.DjName = selectedDj.FirstName + " " + selectedDj.LastName;

            return View(selectedDj);
        }

        public async Task<IActionResult> UpdatePhoneAsync(string phoneNumber)
        {
            ApplicationUser currentUser = await _userManager.GetUserAsync(User);

            currentUser.PhoneNumber = phoneNumber;

            DBcontext.Update(currentUser);
            await DBcontext.SaveChangesAsync();

            return View("Profile");

        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
