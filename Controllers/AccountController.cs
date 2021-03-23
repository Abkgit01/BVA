using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;
using VoucherAutomationSystem.ViewModels;

namespace VoucherAutomationSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly IHostingEnvironment hostingEnvironment;
        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager, IHostingEnvironment hostingEnvironment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            this.hostingEnvironment = hostingEnvironment;
        }

        [Authorize(Roles = "ChiefAccountant, Admin")]
        [HttpGet]
        public IActionResult Registration()
        {
            var roles = roleManager.Roles;
            return View(roles);
        }

        [Authorize(Roles = "ChiefAccountant, Admin")]
        public async Task<JsonResult> Registration(RegisterViewModel registration)
        {
            string imageUrl = SavePhoto(registration.Photo);
            string result = "";
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = registration.Email,
                    Email = registration.Email,
                    FirstName = registration.FirstName,
                    LastName = registration.LastName,
                    SignatureImage = imageUrl

                };
                var isEmailInUse = userManager.Users.FirstOrDefault(x => x.Email == registration.Email);
                if (isEmailInUse != null)
                {
                    result = "2|Email already exist.";
                }
                else
                {
                    var res = await userManager.CreateAsync(user, registration.Password);


                    if (res.Succeeded)
                    {
                        if (registration.Role != "Select Roles")
                        {
                            await userManager.AddToRoleAsync(user, registration.Role);
                            //await signInManager.SignInAsync(user, isPersistent: false);
                            var newUser = await userManager.FindByEmailAsync(user.Email);
                            newUser.IsActive = true;
                            await userManager.UpdateAsync(newUser);

                            result = "1|Account created successfully.";
                        }
                        else
                        {
                            var newUser = await userManager.FindByEmailAsync(user.Email);
                            newUser.IsActive = true;
                            await userManager.UpdateAsync(newUser);
                            //await signInManager.SignInAsync(user, isPersistent: false);
                            result = "1|Account created successfully.";
                        }

                    }
                    else
                    {
                        result = "0|Something went wrong trying to create account.";
                    }
                }
            }
            else
            {
                result = "3|Something went wrong trying to validate account.";
            }
            return Json(result);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel login, string returnUrl)
        {
            string result;
            if (ModelState.IsValid)
            {

                var res = await signInManager.PasswordSignInAsync(login.Email, login.Password, login.RememberMe, false);
                //var user = await userManager.FindByEmailAsync(login.Email);

                if (res.Succeeded )
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        result = $"3|{returnUrl}";
                    }
                    else
                    {
                        result = "1|Account Login successfully.";
                        //return RedirectToAction("DashBoard", "Voucher");
                    }


                }
                else
                {
                    await signInManager.SignOutAsync();
                    result = "0|Password and Login Combination incorrect.";
                    //return View();
                }
            }
            else
            {
                result = "2|Invalid Login Attempt.";
                //return View();
            }
            //return RedirectToAction("DashBoard","Voucher");
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public async Task<IActionResult> EditUser()
        {
            var user = await userManager.GetUserAsync(User);
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel editUserViewModel)
        {
            string result = "";
            var user = await userManager.GetUserAsync(User);
            if (editUserViewModel.Photo != null)
            {
                string imageUrl = SavePhoto(editUserViewModel.Photo);
                user.SignatureImage = imageUrl;
            }
            user.FirstName = editUserViewModel.FirstName;
            user.LastName = editUserViewModel.LastName;
            user.Email = editUserViewModel.Email;
            user.UserName = editUserViewModel.Email;
            
            var res = await userManager.UpdateAsync(user);
            if (res.Succeeded)
            {
                result = "1|Account updated successfully.";
            }
            else
            {
                result = "1|Account didn't update.";
            }
            return Json(result);
        }
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordViewModel)
        {
            string result;
            //var user = await userManager.GetUserAsync(User);
            var user = await userManager.FindByEmailAsync(changePasswordViewModel.Email);
            var res = await userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);
            if (res.Succeeded)
            {
                result = "1|Password changed successfully.";
            }
            else
            {
                result = "2|Password change was not successful.";
            }
            return Json(result);


        }
        private string SavePhoto(IFormFile photo)
        {
            string uploadsFolder = Path.Combine(hostingEnvironment.WebRootPath, "images");
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + photo.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                photo.CopyTo(fileStream);
            }
            return uniqueFileName;
        }

        [HttpGet]
        public IActionResult PasswordResetRequest()
        {
            return View();
        }
        [HttpGet]
        public IActionResult EmailSent()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> PasswordResetRequest(PasswordResetViewModel model)
        {
            string result;
            // Validates the received email address based on the view model
            if (!ModelState.IsValid)
            {
                result = "2|Email not valid.";
            }

            // Gets the user entity for the specified email address
            var user = await userManager.FindByEmailAsync(model.Email);
            //var user = await userManager.Users.FirstOrDefaultAsync(x => x.Email == model.Email);

            if (user != null)
            {
                // Generates a password reset token for the user
                string token = await userManager.GeneratePasswordResetTokenAsync(user);

                // Prepares the URL of the password reset link (targets the "ResetPassword" action)
                // Fill in the name of your controller
                string resetUrl = Url.Action("PasswordReset", "Account", new { token, user.Email }, Request.Scheme);

                // Creates and sends the password reset email to the user's address
                SendMail(user,  resetUrl);
                result = "1|Check your mail for reset link.";
            }
            else
            {
                result = "2|User with this mail not found.";
            }
            return Json(result);
        }

        [HttpGet]
        public IActionResult PasswordReset(string token, string email)
        {
            var model = new PasswordResetViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasswordReset(PasswordResetViewModel passwordResetViewModel)
        {
            if (!ModelState.IsValid)
                return View(passwordResetViewModel);
            var user = await userManager.FindByEmailAsync(passwordResetViewModel.Email);
            if (user == null)
                RedirectToAction(nameof(passwordResetViewModel));
            var resetPassResult = await userManager.ResetPasswordAsync(user, passwordResetViewModel.Token, passwordResetViewModel.Password);
            if (!resetPassResult.Succeeded)
            {
                foreach (var error in resetPassResult.Errors)
                {
                    ModelState.TryAddModelError(error.Code, error.Description);
                }
                return View();
            }
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public string SendMail(ApplicationUser user,  string url)
        {
            MailModel mailModel = new MailModel();
            var result = "";

                string msg = "To reset your account's password, click " + url;
                MailMessage mail = new MailMessage();


                mail.To.Add(user.Email);
                mail.From = new MailAddress("support@brandonetech.com");
                mail.Subject = "Password Reset";
                //string Body = mailModel.Body;
                mail.Body = msg;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtppro.zoho.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("support@brandonetech.com", "*123*brandonetech#"); // Enter senders User name and password       
                smtp.EnableSsl = true;
                smtp.Send(mail);

            return result;
        }
    }
}
