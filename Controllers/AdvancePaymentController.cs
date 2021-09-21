using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
//using System.Web.Mvc;
using VoucherAutomationSystem.Data;
using VoucherAutomationSystem.Models;
using VoucherAutomationSystem.Models.Enums;
using VoucherAutomationSystem.ViewModels;

namespace VoucherAutomationSystem.Controllers
{
    [Authorize(Roles = "AccountOfficer, ChiefAccountant,Approval,Authorizer1,Authorizer2,Staff")]
    public class AdvancePaymentController : Controller
    {
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IAdvancePaymentService advanceService;
        private readonly AppDbContext context;

        public AdvancePaymentController(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IAdvancePaymentService advanceService, AppDbContext context)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.advanceService = advanceService;
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create() 
        {
            var user = await userManager.GetUserAsync(User);
            var departments = await context.Departments.ToListAsync();
            var userDept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            return View(new AdvanceCreateViewModel {Dept = departments, FirstName = user.FirstName, LastName = user.LastName , userDept = userDept.Name} );
        }

        [HttpPost]
        public async Task<IActionResult> Create(CashAdvanceViewModel cashAdvance, List<CashAdvancePaymentsViewModel> cashAdvancePayments)
        {
            try
            {
                string result;
                var user = await userManager.GetUserAsync(User);
                if (user.IsActive == false)
                {
                    await signInManager.SignOutAsync();
                    return RedirectToAction("Login", "Account");
                }
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

                var res = await advanceService.AddNewCashAdvance(user.Id, role.Id, cashAdvance, cashAdvancePayments);
                //var file = await AddFiles(res.Id, uploadfiles);
                if (res != null)
                {
                    IEnumerable<ApplicationUser> mailUsers = null;
                    if (res.CurrentStage == 2)
                    {
                        mailUsers = await userManager.Users.Where(x => x.DeptId == res.DeptId && x.RoleLead == true).ToListAsync();
                    }
                    if (res.CurrentStage == 3)
                    {
                        mailUsers = await userManager.GetUsersInRoleAsync(GeneralClass.ChiefAccountant);
                    }
                    if (res.CurrentStage == 4)
                    {
                        mailUsers = await userManager.GetUsersInRoleAsync(GeneralClass.Authorizer1);
                    }
                    await SendMail(res, mailUsers.ToList(), cashAdvance.Comment);
                    result = "1|" + res.Id + "|Cash Advance created successfully!";
                }
                else
                {
                    result = "2|Error creating cash advance.";
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                var msg = "2|" + ex.Message;
                return Json(msg);
            }

        }

        [HttpGet]
        public async Task<IActionResult> EditCashAdvance(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var dept = await context.Departments.ToListAsync();
            var cashAdvance = await context.CashAdvances.FirstOrDefaultAsync(x => x.Id == Id);
            var cashAdvancePayment = await context.CashAdvancePayments.Where(x => x.CashAdvanceId == Id).ToListAsync();
            if (cashAdvance.UserId == user.Id)
            {
                return View(new CashCompleteViewModel { cashAdvance = cashAdvance, cashAdvancePayment = cashAdvancePayment, department = dept, UserId = user.Id});
            }
            return RedirectToAction("AdvancePending","AdvancePayment");
        }

        [HttpPost]
        public async Task<IActionResult> EditCashAdvance(CashAdvance cashAdvance, List<CashAdvancePayment> cashAdvancePayments, string comment)
        {
            try
            {
                var result = "";
                var user = await userManager.GetUserAsync(User);
                if (user.IsActive == false)
                {
                    await signInManager.SignOutAsync();
                    return RedirectToAction("Login", "Account");
                }
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

                var res = await advanceService.EditCashAdvance(cashAdvance, cashAdvancePayments, user.Id, role.Id, comment);
                if (res != null)
                {
                    IEnumerable<ApplicationUser> mailUsers = null;
                    if (res.CurrentStage == 2)
                    {
                        mailUsers = await userManager.Users.Where(x => x.DeptId == res.DeptId && x.RoleLead == true).ToListAsync();
                    }
                    if (res.CurrentStage == 3)
                    {
                        mailUsers = await userManager.GetUsersInRoleAsync(GeneralClass.ChiefAccountant);
                    }
                    if (res.CurrentStage == 4)
                    {
                        mailUsers = await userManager.GetUsersInRoleAsync(GeneralClass.Authorizer1);
                    }
                    if (res.CurrentStage == 5)
                    {
                        mailUsers = await userManager.GetUsersInRoleAsync(GeneralClass.Authorizer2);
                    }
                    await SendMail(res, mailUsers.ToList(), comment);
                    result = "1|" + res.Id + "|Advance Payment updated successfully!";
                }
                else
                {
                    result = "2|Error updating Advance payment.";
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                var msg = "2|" + ex.Message;
                return Json(msg);
            }

        }

        [HttpGet]
        public async Task<ViewResult> AdvancePaymentAction(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var cashAdvance = await advanceService.GetCashAdvance(Id, user.Id);
            var cashAdvancePayments = await advanceService.GetCashAdvancePayments(Id);
            var actions = await advanceService.GetCashAdvanceAction(Id);
            //add department
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == cashAdvance.DeptId);
            cashAdvance.Dept = dept;
            foreach (var action in actions)
            {
                action.User = await userManager.Users.FirstOrDefaultAsync(x => x.Id == action.UserId);
                //action.User = await userManager.Users.FirstOrDefaultAsync(u => u.Id == action.Id);
            }
            return View(new CashCompleteViewModel { cashAdvance = cashAdvance, cashAdvancePayment = cashAdvancePayments, cashAdvanceActions = actions.OrderBy(x => x.DateUpdated).ToList(), UserId = user.Id, User = user });
        }

        [HttpPost]
        public async Task<IActionResult> AdvancePaymentAction(CashCompleteViewModel cashComplete)
        {
            try
            {
                string result = "";
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                if (user.IsActive == false)
                {
                    await signInManager.SignOutAsync();
                    return RedirectToAction("Login", "Account");
                }

                var res = await advanceService.PerformActionOnCashAdvance(cashComplete.Id, user.Id, role.Id, cashComplete.comment, (ActionPerformed)cashComplete.ActionPerformed);
                if (res != null)
                {
                    if (res.IsApproved == false)
                    {
                        IEnumerable<ApplicationUser> mailUsers = null;
                        if (res.CurrentStage == 1)
                        {
                            mailUsers = await userManager.Users.Where(x => x.Id == res.UserId).ToListAsync();
                        }
                        if (res.CurrentStage == 2)
                        {
                            mailUsers = await userManager.Users.Where(x => x.DeptId == res.DeptId && x.RoleLead == true).ToListAsync();
                        }
                        if (res.CurrentStage == 3)
                        {
                            mailUsers = await userManager.GetUsersInRoleAsync(GeneralClass.ChiefAccountant);
                        }
                        if (res.CurrentStage == 4)
                        {
                            mailUsers = await userManager.GetUsersInRoleAsync(GeneralClass.Authorizer1);
                        }
                        if (res.CurrentStage == 5)
                        {
                            mailUsers = await userManager.GetUsersInRoleAsync(GeneralClass.Authorizer2);
                        }
                        await SendMail(res, mailUsers.ToList(), cashComplete.comment);
                        if (cashComplete.ActionPerformed == 2)
                        {
                            result = "1|Approval successfull!";
                        }
                        else
                        {
                            result = "1|Rejection successfull!";
                        }
                    }
                    else
                    {
                        HashSet<ApplicationUser> mailUsers = new HashSet<ApplicationUser>();
                        var actions = await context.CashAdvanceActions.Where(x => x.CashAdvanceId == res.Id).ToListAsync();
                        foreach (var action in actions)
                        {
                            mailUsers.Add(await context.Users.FirstOrDefaultAsync(x => x.Id == action.UserId));
                        }
                        await SendMail(res, mailUsers.ToList(), cashComplete.comment);
                        if (cashComplete.ActionPerformed == 2)
                        {
                            result = "1|Approval successfull!";
                        }
                        else
                        {
                            result = "1|Rejection successfull!";
                        }
                    }
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                var msg = "2|" + ex.Message;
                return Json(msg);
            }
            
        }

        [HttpGet]
        public async Task<ViewResult> GetAdvancePayment(int Id)
        {
            var user = await userManager.GetUserAsync(User);

            var cashAdvance = await advanceService.GetCashAdvance(Id, user.Id);
            var cashPayments = await advanceService.GetCashAdvancePayments(Id);
            if (cashAdvance == null)
            {
                throw new Exception("Advance payment not found.");
            }

            return View(new CashCompleteViewModel { cashAdvance = cashAdvance, cashAdvancePayment = cashPayments });
        }

        
        [HttpGet]
        public async Task<IActionResult> PendingAdvancePayment()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var res = await advanceService.GetCashAdvanceForRole(role.Id, user.Id);
            return View(res.OrderByDescending(x => x.DateCreated));
        }

        [HttpGet]
        public async Task<IActionResult> AllAdvancePayment()
        {
            IEnumerable<CashAdvance> res = null;
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
            if (role.Name == "ChiefAccountant" || role.Name == "Authorizer1" || role.Name == "Authorizer2" || role.Name == "AccountOfficer" || role.Name == "Approval")
            {
                res = await advanceService.GetAllCashAdvance(user.Id);
            }
            else
            {
                res = await advanceService.GetAllCashAdvanceCreatedByUser(user.Id);
            }
            
            foreach (var cash in res)
            {
                var cashUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == cash.UserId);
                cash.User = cashUser;
            }
            return View(new ViewAllCashViewModel { cashAdvances = res.OrderByDescending(x => x.DateCreated).ToList() });
        }

        [Authorize(Roles = " ChiefAccountant,Authorizer1,Authorizer2,AccountOfficer")]
        [HttpGet]
        public async Task<IActionResult> ViewApprovedCashAdvance()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            if (user != null)
            {
                var res = await advanceService.GetActiveCashAdvance(user.Id);
                return View(res.OrderByDescending(x => x.DateCreated).ToList());
            }
            else
            {
                return RedirectToAction("Dashboard", "Voucher");
            }
        }

        [Authorize(Roles = " ChiefAccountant,Authorizer1,Authorizer2,AccountOfficer")]
        [HttpGet]
        public async Task<IActionResult> ViewUnApprovedCashAdvance()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var res = await advanceService.GetInActiveCashAdvance(user.Id);
                return View(res.OrderByDescending(x => x.DateCreated).ToList());
            }
            catch (Exception)
            {
                return RedirectToAction("Dashboard", "Voucher");
            }

        }

        [HttpGet]
        public ViewResult AddFiles(int Id)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddFiles(int advancePaymentId, List<IFormFile> files)
        {
            string result;
            try
            {
                if (files == null)
                {
                    result = "1|Advance payment updated successfully!";
                }
                else
                {
                    var user = await userManager.GetUserAsync(User);
                    var roles = await userManager.GetRolesAsync(user);
                    var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

                    var res = await advanceService.AddCashAdvanceFiles(advancePaymentId, files, user.Id, role.Id);
                    if (res != null)
                    {
                        result = "1|File(s) successfully saved!";
                    }
                    else
                    {
                        result = "0|File(s) failed to save!";
                    }
                }
                return Json(result);
            }
            catch (Exception ex)
            {
                var msg = ("0|" + ex.Message);
                return Json(msg);
            }

        }

        [HttpGet]
        public async Task<ViewResult> GetAdvancePaymentFiles(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var advanceFiles = await advanceService.GetCashAdvanceFiles(Id);
            foreach (var file in advanceFiles)
            {
                file.CashAdvance = await context.CashAdvances.FirstOrDefaultAsync(x => x.Id == file.CashAdvanceId);
            }
            return View( new AdvanceFilesViewModel { CashAdvanceFiles = advanceFiles, UserId = user.Id});
        }
        
        public async Task<IActionResult> DeleteAdvancePaymentFile(int Id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await advanceService.DeleteCashAdvanceFile(Id, user.Id, role.Id);
                return RedirectToAction("GetAdvancePaymentFiles", new { id = res.CashAdvanceId });
            }
            catch (Exception ex)
            {
                var msg = ("0|" + ex.Message);
                return View("AllAdvancePayment");
            }
        }

        public async Task<IActionResult> DeletePayment(int Id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await advanceService.DeleteAdvancePayments(user.Id, role.Id, Id);
                return RedirectToAction("EditCashAdvance", new { id = res.CashAdvanceId });
            }
            catch (Exception ex)
            {
                var msg = "2|" + ex.Message;
                return Json(msg);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SearchByDate(SearchBydateViewModel searchBydateViewModel)
        {
            var user = await userManager.GetUserAsync(User);

            if (searchBydateViewModel.MinimumDate > searchBydateViewModel.MaximumDate)
                throw new ArgumentException("From Date cannot be greater than To Date");
            var searchedCashAdvance = new List<CashAdvance>();
            var newCashAdvances = await advanceService.GetAllCashAdvance(user.Id);
            foreach (var cashAdvance in newCashAdvances)
            {
                if (cashAdvance.DateCreated >= searchBydateViewModel.MinimumDate && cashAdvance.DateCreated <= searchBydateViewModel.MaximumDate)
                {
                    searchedCashAdvance.Add(cashAdvance);
                }
            }
            //context.Entry(vouchers).State = EntityState.Detached;
            return View("AllAdvancePayment", new ViewAllCashViewModel { cashAdvances = searchedCashAdvance.OrderByDescending(x => x.DateUpdated).ToList() });
        }

        public async Task<ActionResult> ViewCashAdvance(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
            if (role != null)
            {
                var cashAdvance = await advanceService.GetCashAdvance(Id, user.Id);
                if (cashAdvance == null)
                {
                    return RedirectToAction("DashBoard", "Voucher");
                }
                var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == cashAdvance.DeptId);
                cashAdvance.Dept = dept;
                var payments = await advanceService.GetCashAdvancePayments(Id);
                var actions = await advanceService.GetCashAdvanceAction(Id);
                var cashAdvanceFiles = await advanceService.GetCashAdvanceFiles(Id);
                foreach (var cashAdvanceFile in cashAdvanceFiles)
                {
                    cashAdvanceFile.CashAdvance.UserId = cashAdvance.UserId;
                }
                foreach (var action in actions)
                {
                    action.User = await userManager.FindByIdAsync(action.UserId.ToString());
                    //action.User = await userManager.Users.FirstOrDefaultAsync(u => u.UserId == action.Id);
                }
                List<CashAdvanceAction> ActionList = new List<CashAdvanceAction>();
                foreach (var action in actions.OrderByDescending(x => x.DateUpdated))
                {

                    ActionList.Add(action);
                    if (action.ActionPerformed == ActionPerformed.Edited || action.ActionPerformed == ActionPerformed.Created)
                    {
                        break;
                    }

                }
                string numberInWords = NumberToWords.ConvertAmount(Convert.ToDouble(cashAdvance.TotalAmount.Value));
                foreach (var action in actions)
                {
                    if (action.UserId == user.Id)
                    {
                        return View("ViewCashAdvance", new CashAdvancePaymentsViewModel { cashAdvance = cashAdvance, cashAdvancePayments = payments, cashAdvanceActions = ActionList.OrderBy(x => x.DateUpdated), NumberInWords = numberInWords, cashAdvanceFiles = cashAdvanceFiles });
                    }
                }
                if (await userManager.IsInRoleAsync(user, "AccountOfficer"))
                {
                    return View("ViewCashAdvance", new CashAdvancePaymentsViewModel { cashAdvance = cashAdvance, cashAdvancePayments = payments, cashAdvanceActions = ActionList.OrderBy(x => x.DateUpdated), NumberInWords = numberInWords, cashAdvanceFiles = cashAdvanceFiles });
                }
                return RedirectToAction("DashBoard", "Voucher");
                
                
            }
            return View();
        }

        [HttpGet]
        public async Task<string> SendMail(CashAdvance cashAdvance, List<ApplicationUser> users, string comment)
        {
            MailModel mailModel = new MailModel();
            var result = "";

            if (cashAdvance.IsApproved == false)
            {
                string msg = $"Dear Sir/Ma, you have a cash advance on your desk. Click to view https://bva.azurewebsites.net/AdvancePayment/AdvancePaymentAction/{cashAdvance.Id}"  + " . Comment: " + comment;
                MailMessage mail = new MailMessage();
                    foreach (var user in users)
                    {
                        if (user.IsActive == true)
                        {
                            mail.To.Add(user.Email);
                        }
                    }

                mail.From = new MailAddress("support@brandonetech.com" );
                mail.Subject = "NEW CASH ADVANCE SLIP ON YOUR DESK";
                //string Body = mailModel.Body;
                mail.Body = msg;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtppro.zoho.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("support@brandonetech.com", "*123*brandonetech#" ); // Enter senders User name and password       
                smtp.EnableSsl = true;
                smtp.Send(mail);
                //return result;
            }
            else if (cashAdvance.IsApproved == true)
            {
                string msg = $"This Cash Advance has been approved, click to view. https://bva.azurewebsites.net/AdvancePayment/ViewCashAdvance/{cashAdvance.Id}"  + " . Comment: " + comment;
                MailMessage mail = new MailMessage();
                foreach (var user in users)
                {
                    mail.To.Add(user.Email);
                }
                mail.From = new MailAddress("support@brandonetech.com" );
                mail.Subject = "CASH ADVANCE APPROVED";
                //string Body = mailModel.Body;
                mail.Body = msg;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtppro.zoho.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("support@brandonetech.com", "*123*brandonetech#" ); // Enter senders User name and password       
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            return result;
        }

        
    }
}
