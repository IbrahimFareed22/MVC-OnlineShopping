using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using OnlineShopping.Data;
using OnlineShopping.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;


namespace OnlineShopping.Controllers
{
    //[Authorize(Roles = "admin")]

    public class UserController : Controller
    {
        private readonly OnlineShoppingContext _context;
        public UserController(OnlineShoppingContext context)
        {
            _context = context;
        }

        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        //[Bind("Email,Password,ConfirmPassword")]
        public async Task<IActionResult> Register( UserViewModel userView)
        {
            if (ModelState.IsValid)
            {
                if (userView.Password != userView.ConfirmPassword)
                {
                    ViewData["ErrorMessage"] = "Passwords do not match.";
                    return View(userView);
                }

                bool userExists = await _context.User.AnyAsync(u => u.Email == userView.Email);
                if (userExists)
                {
                    ViewData["ErrorMessage"] = "User already exists.";
                    return View(userView);
                }

                var user = new User
                {
                    Email = userView.Email,
                    Password = BCrypt.Net.BCrypt.HashPassword(userView.Password),
                    Role = userView.Role, // ← خد الـ Role من اليوزر
                    //Role = "normal",
                    Status = true // أو false لو عايز تفعّل حسابه يدوي

                };

                _context.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "User registered successfully. Please login.";
            }
            return RedirectToAction("Login", "User");
            //return View(userView);
        }

        // GET: User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel loginView)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.Email == loginView.Email);

                if (user == null)
                {
                    ViewData["ErrorMessage"] = "Invalid credentials.";
                    return View(loginView);
                }

                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(loginView.Password, user.Password);

                if (!isPasswordValid)
                {
                    ViewData["ErrorMessage"] = "Invalid credentials.";
                    return View(loginView);
                }

                if (!user.Status)
                {
                    ViewData["ErrorMessage"] = "Your account is inactive.";
                    return View(loginView);
                }

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties { IsPersistent = true });

                // تخزين معلومات المستخدم في الجلسة
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("UserName", user.Email);
                HttpContext.Session.SetString("UserRole", user.Role);

                // التوجيه حسب الدور
                if (user.Role == "admin")
                {
                    return RedirectToAction("Index", "Product");
                }
                else
                {
                    return RedirectToAction("ProductDashboard", "Product");
                }
            }

            return View(loginView);
        }


        // GET: User/Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        // GET: User/Logout
        public async Task<IActionResult> Logout()
        {
            // Clear session
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "User"); // أو رجعه مثلا للصفحة الرئيسية

        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            return View(await _context.User.ToListAsync());
        }


        // GET: User/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }


        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserName,Password,Role,Status")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }


        // GET: User/Edit/5
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

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserName,Password,Role,Status")] User user)
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
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: User/Delete/5
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: User/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        //[AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}