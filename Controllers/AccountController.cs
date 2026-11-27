using System.Text;
using System.Text.Encodings.Web;
using Histador.Models;
using Histador.Services;
using Histador.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Histador.Controllers
{
    public class AccountController(
        UserManager<ApplicationUser> users,
        SignInManager<ApplicationUser> signIn,
        AccountService accounts,
        IEmailSender email) : Controller
    {
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null) => View(new LoginViewModel { ReturnUrl = returnUrl });

        [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await signIn.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);
            if (result.Succeeded)
                return LocalRedirect(SafeReturnUrl(model.ReturnUrl));

            ModelState.AddModelError(string.Empty, result.IsLockedOut
                ? "Too many failed attempts. Try again in a few minutes."
                : "Invalid email or password.");
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult Register() => View(new RegisterViewModel());

        [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await accounts.RegisterAsync(model.Email, model.DisplayName, model.Password, model.PreferredLanguage);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    var field = error.Code switch
                    {
                        "DuplicateDisplayName" or "InvalidDisplayNameLength" => nameof(model.DisplayName),
                        "DuplicateEmail" or "DuplicateUserName" or "InvalidEmail" or "InvalidUserName" => nameof(model.Email),
                        _ when error.Code.StartsWith("Password") => nameof(model.Password),
                        _ => string.Empty,
                    };
                    ModelState.AddModelError(field, error.Code is "DuplicateUserName" ? $"Email '{model.Email}' is already registered." : error.Description);
                }
                return View(model);
            }

            var user = await users.FindByEmailAsync(model.Email);
            await signIn.SignInAsync(user!, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await signIn.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Same response whether or not the account exists, so emails can't be probed.
            var user = await users.FindByEmailAsync(model.Email);
            if (user is not null)
            {
                var token = await users.GeneratePasswordResetTokenAsync(user);
                var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var link = Url.Action(nameof(ResetPassword), "Account", new { code, email = user.Email }, Request.Scheme)!;
                await email.SendAsync(user.Email!, "Reset your Histador password",
                    $"Reset your password by <a href=\"{HtmlEncoder.Default.Encode(link)}\">clicking here</a>.");
            }

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [AllowAnonymous]
        public IActionResult ForgotPasswordConfirmation() => View();

        [AllowAnonymous]
        public IActionResult ResetPassword(string? code = null, string? email = null)
        {
            if (code is null)
                return BadRequest("A reset code is required.");
            return View(new ResetPasswordViewModel { Code = code, Email = email ?? string.Empty });
        }

        [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await users.FindByEmailAsync(model.Email);
            if (user is null)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            string token;
            try
            {
                token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Code));
            }
            catch (FormatException)
            {
                ModelState.AddModelError(string.Empty, "This reset link is invalid. Request a new one.");
                return View(model);
            }

            var result = await users.ResetPasswordAsync(user, token, model.Password);
            if (result.Succeeded)
                return RedirectToAction(nameof(ResetPasswordConfirmation));

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Code == "InvalidToken" ? "This reset link is invalid or has expired. Request a new one." : error.Description);
            return View(model);
        }

        [AllowAnonymous]
        public IActionResult ResetPasswordConfirmation() => View();

        public IActionResult ChangePassword() => View(new ChangePasswordViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await users.GetUserAsync(User);
            if (user is null)
                return Challenge();

            var result = await users.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(error.Code == "PasswordMismatch" ? nameof(model.OldPassword) : string.Empty,
                        error.Code == "PasswordMismatch" ? "Current password is incorrect." : error.Description);
                return View(model);
            }

            await signIn.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Your password has been changed.";
            return RedirectToAction(nameof(ChangePassword));
        }

        private string SafeReturnUrl(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
    }
}
