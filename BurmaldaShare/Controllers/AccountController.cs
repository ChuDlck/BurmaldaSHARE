using Microsoft.AspNetCore.Mvc;
using BurmaldaSHARE.Services;
using BurmaldaSHARE.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace BurmaldaSHARE.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)//Автоматом подсасывает UserServices
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult Register()//GET, открытие окна регистрации
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string login, string password)
        {
            try
            {
                User newUser = _userService.Register(login, password);
                await SignInUserAsync(newUser);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Login(string login, string password) //выход из аккаунта
        {

            var user = _userService.Authenticate(login, password);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Неверный логин или пароль";
                return View();
            }
            await SignInUserAsync(user);
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        /// <summary>
        /// Создание куки только с логином юзера
        /// </summary>
        /// <param name="user">сам юзер</param>
        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Login)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));
        }
    }
}
