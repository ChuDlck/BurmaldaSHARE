using Microsoft.AspNetCore.Mvc;
using BurmaldaSHARE.Services;
using BurmaldaSHARE.Models;

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
        public IActionResult Register(string login, string password) // POST, регистрация
        {
            try
            {
                _userService.Register(login, password);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)//обработчик ошибок
            {
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }
    }
}
