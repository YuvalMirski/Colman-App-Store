﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ColmanAppStore.Data;
using ColmanAppStore.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace ColmanAppStore.Controllers
{
    public class UsersController : Controller
    {
        private readonly ColmanAppStoreContext _context;

        public UsersController(ColmanAppStoreContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Logout()
        {
            //HttpContext.Session.Clear();

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login");
        }

        //GET: Users/Register
        public IActionResult Register()
        {
            return View();

        }



        // POST: Users/Register
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("Id,Name,Email,Password,UserType")] User user)
        {

            if (ModelState.IsValid)
            {
                var q = _context.User.FirstOrDefault(u => u.Email == user.Email || u.Name == user.Name);

                if (q == null)
                {
                    _context.Add(user);
                    await _context.SaveChangesAsync();

                    var u = _context.User.FirstOrDefault(u => u.Email == user.Email && u.Password == user.Password);
                    Signin(u);


                    //return RedirectToAction("HomePage", "Apps");
                    return RedirectToAction("Create", "PaymentMethods");
                }
                else
                {
                    ViewData["Error"] = "Unable to comply, cannot register this user.";
                }
            }

            return View(user);
        }

        // GET: Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Users/Login
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Id,Name, Email,Password")] User user)
        {

            if (ModelState.IsValid)
            {

                var q = from u in _context.User
                        where u.Password == user.Password && u.Email == user.Email
                        select u;

                if (q.Count() > 0)
                {
                    Signin(q.First());
                    return RedirectToAction("HomePage", "Apps");
                }
                else
                {
                    ViewData["Error"] = "Password and/or Email are incorrect.";
                }
            }
            return View(user);
        }

        private async void Signin(User account)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, account.Name),
                    new Claim(ClaimTypes.Role, account.UserType.ToString()),
                };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10)
            };

            await HttpContext.SignInAsync(
                  CookieAuthenticationDefaults.AuthenticationScheme,
                  new ClaimsPrincipal(claimsIdentity),
                  authProperties);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
           
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,Password,UserType")] User user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("HomePage", "Apps");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            return View(user);
        }
        public async Task<IActionResult> Account(string id)//get to user account info by name
        {

            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.Include(x => x.PaymentMethods).Include(x => x.AppListUser).FirstOrDefaultAsync(m => m.Name == id);
            

            if (user == null)
            {
                return NotFound();
            }



            return View(user);


        }

        public async Task<IActionResult> PopularityGraph()
        {
            int len = _context.User.Count();//total users
           for (int i=0; i<len; i++)
           {
               int numUserApps= _context.User.ToList().ElementAt(i).AppListUser.Count();//users apps
                for(int j=0; j< numUserApps; j++)
                {
                    //......
                }

           }
            return View();

        }
            private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

    }
}