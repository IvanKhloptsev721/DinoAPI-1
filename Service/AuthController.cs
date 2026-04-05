using Microsoft.AspNetCore.Mvc;
using DinoApp.Services;
using DinoApp.Models;
using DinoApp.Attributes;
using Microsoft.AspNetCore.Authorization;

namespace DinoApp.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_authService.IsAuthenticated())
            return RedirectToAction("Index", "Dinosaurs");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.LoginAsync(model.Username, model.Password, model.RememberMe);

            if (result)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                TempData["SuccessMessage"] = $"Добро пожаловать, {model.Username}!";
                return RedirectToAction("Index", "Dinosaurs");
            }

            ModelState.AddModelError("", "Неверное имя пользователя или пароль");
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (_authService.IsAuthenticated())
            return RedirectToAction("Index", "Dinosaurs");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterUserDto model)
    {
        if (ModelState.IsValid)
        {
            var result = await _authService.RegisterAsync(model);

            if (result)
            {
                // Автоматически входим после регистрации
                await _authService.LoginAsync(model.Username, model.Password, false);
                TempData["SuccessMessage"] = "Регистрация прошла успешно! Добро пожаловать в Dino_Mir!";
                return RedirectToAction("Index", "Dinosaurs");
            }

            ModelState.AddModelError("", "Пользователь с таким именем или email уже существует");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Logout()
    {
        _authService.Logout();
        TempData["SuccessMessage"] = "Вы успешно вышли из системы";
        return RedirectToAction("Index", "Dinosaurs");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [Authorize]
    public IActionResult Profile()
    {
        var user = _authService.GetCurrentUser();
        if (user == null)
            return RedirectToAction("Login");

        return View(user);
    }
}