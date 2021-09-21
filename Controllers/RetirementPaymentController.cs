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
using VoucherAutomationSystem.Data;
using VoucherAutomationSystem.Models;
using VoucherAutomationSystem.Models.Enums;
using VoucherAutomationSystem.ViewModels;

namespace VoucherAutomationSystem.Controllers
{
    [Authorize(Roles = "AccountOfficer, ChiefAccountant,Approval,Authorizer1,Authorizer2,Staff")]
    public class RetirementPaymentController : Controller
    {
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IRetirementPaymentService retirementService;
        private readonly IAdvancePaymentService advancePaymentService;
        private readonly AppDbContext context;

        public RetirementPaymentController(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IRetirementPaymentService retirementPaymentService, AppDbContext context, IAdvancePaymentService advancePaymentService)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.retirementService = retirementPaymentService;
            this.context = context;
            this.advancePaymentService = advancePaymentService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int Id)
        {
            var retire = await context.RetirementPayments.FirstOrDefaultAsync(x => x.CashAdvanceId == Id);
            if (retire != null)
            {
                return RedirectToAction("Dashboard","Voucher");
            }
            var user = await userManager.GetUserAsync(User);
            var departments = await context.Departments.ToListAsync();
            var cashAdvance = await context.CashAdvances.FirstOrDefaultAsync(x => x.Id == Id);
            var cashAdvancePayment = await context.CashAdvancePayments.Where(x => x.CashAdvanceId == Id).ToListAsync();
            var userDept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            return View(new RetirementCreateViewModel { Dept = departments, FirstName = user.FirstName, LastName = user.LastName, userDept = userDept.Name, cashAdvance = cashAdvance, cashAdvancePayments = cashAdvancePayment });
        }

        [HttpPost]
        public async Task<IActionResult> Create(RetirementPaymentViewModel retirementPaymentViewModel, List<RetirementCashPaymentsViewModel> retirementCashPayments)
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

                var res = await retirementService.AddRetirementPayment(user.Id, role.Id, retirementPaymentViewModel, retirementCashPayments, retirementPaymentViewModel.cashAdvanceId);
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
                    await SendMail(res, mailUsers.ToList(), retirementPaymentViewModel.Comment);
                    result = "1|" + res.Id + "|Retirement Payment created successfully!";
                }
                else
                {
                    result = "2|Error creating retirement payment.";
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
        public async Task<IActionResult> CreateCashRetirement()
        {
            
            var user = await userManager.GetUserAsync(User);
            var departments = await context.Departments.ToListAsync();
            
            var userDept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            return View(new RetirementCreateViewModel { Dept = departments, FirstName = user.FirstName, LastName = user.LastName, userDept = userDept.Name });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCashRetirement(RetirementPaymentViewModel retirementPaymentViewModel, List<RetirementCashPaymentsViewModel> retirementCashPayments)
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

                var res = await retirementService.CreateRetirementPayment(user.Id, role.Id, retirementPaymentViewModel, retirementCashPayments, retirementPaymentViewModel.cashAdvanceId);
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
                    await SendMail(res, mailUsers.ToList(), retirementPaymentViewModel.Comment);
                    result = "1|" + res.Id + "|Retirement Payment created successfully!";
                }
                else
                {
                    result = "2|Error creating retirement payment.";
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
        public async Task<IActionResult> EditRetirementPayment(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var dept = await context.Departments.ToListAsync();
            var retirementPayment = await context.RetirementPayments.FirstOrDefaultAsync(x => x.Id == Id);
            var cashAdvance = await context.CashAdvances.FirstOrDefaultAsync(x => x.Id == retirementPayment.CashAdvanceId);
            var retirementCashBookPayments = await context.RetirementCashBookPayments.Where(x => x.RetirePaymentId == Id).ToListAsync();
            if (retirementPayment.UserId == user.Id)
            {
                return View(new RetirementCompleteViewModel { retirementPayment = retirementPayment, retirementCashBookPayments = retirementCashBookPayments, departments = dept, UserId = user.Id, cashAdvance = cashAdvance });
            }
            return RedirectToAction("AdvancePending", "RetirementPayment");
        }

        [HttpPost]
        public async Task<IActionResult> EditRetirementPayment(RetirementPayment retirementPayment, List<RetirementCashBookPayments> retirementCashBooks, string comment)
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

                var res = await retirementService.EditRetirePayment(retirementPayment, retirementCashBooks, user.Id, role.Id, comment);
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
                    result = "1|" + res.Id + "|Retirement Payment updated successfully!";
                }
                else
                {
                    result = "2|Error updating Retirement Payment.";
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
        public async Task<ViewResult> RetirePaymentAction(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var retirementPayment = await retirementService.GetRetirePayment(Id, user.Id);
            var retirementCashBooks = await retirementService.GetCashRetirePayment(Id);
            //var cashAdvance = await advancePaymentService.GetCashAdvance(retirementPayment.CashAdvanceId, user.Id);
            var cashBookPayments = await advancePaymentService.GetCashAdvancePayments(retirementPayment.CashAdvanceId);
            var actions = await retirementService.GetRetirePaymentAction(Id);
            //add department
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == retirementPayment.DeptId);
            retirementPayment.Dept = dept;
            decimal? retireCash = System.Math.Abs(Convert.ToDecimal(retirementPayment.TotalRetirementAmount - retirementPayment.CashAdvanceAmount));
            string numberInWords = NumberToWords.ConvertAmount(Convert.ToDouble(retirementPayment.CashAdvanceAmount - retirementPayment.TotalRetirementAmount.Value));
            foreach (var action in actions)
            {
                action.User = await userManager.Users.FirstOrDefaultAsync(x => x.Id == action.UserId);
                //action.User = await userManager.Users.FirstOrDefaultAsync(u => u.Id == action.Id);
            }
            return View(new RetirementCompleteViewModel { retirementPayment = retirementPayment, retirementCashBookPayments = retirementCashBooks, retirementPaymentActions = actions.OrderBy(x => x.DateUpdated).ToList(), UserId = user.Id, User = user , cashAdvancePayments = cashBookPayments, retireCash = retireCash});
        }

        [HttpPost]
        public async Task<IActionResult> RetirementPaymentAction(RetirementCompleteViewModel retirementComplete)
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

                var res = await retirementService.PerformActionOnRetirePayment(retirementComplete.Id, user.Id, role.Id, retirementComplete.comment, (ActionPerformed)retirementComplete.ActionPerformed);
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
                        await SendMail(res, mailUsers.ToList(), retirementComplete.comment);
                        if (retirementComplete.ActionPerformed == 2)
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
                        var actions = await context.RetirementPaymentActions.Where(x => x.RetirementPaymentId == res.Id).ToListAsync();
                        foreach (var action in actions)
                        {
                            mailUsers.Add(await context.Users.FirstOrDefaultAsync(x => x.Id == action.UserId));
                        }
                        await SendMail(res, mailUsers.ToList(), retirementComplete.comment);
                        if (retirementComplete.ActionPerformed == 2)
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
        public async Task<ViewResult> GetRetirementPayment(int Id)
        {
            var user = await userManager.GetUserAsync(User);

            var retirementPayment = await retirementService.GetRetirePayment(Id, user.Id);
            var retirementCashBooks = await retirementService.GetCashRetirePayment(Id);
            if (retirementPayment == null)
            {
                throw new Exception("Retirement Payment not found.");
            }

            return View(new RetirementCompleteViewModel { retirementPayment = retirementPayment, retirementCashBookPayments = retirementCashBooks });
        }

        [HttpGet]
        public async Task<IActionResult> PendingRetirementPayment()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var res = await retirementService.GetRetirePaymentsForRole(role.Id, user.Id);
            return View(res.OrderByDescending(x => x.DateCreated));
        }

        [HttpGet]
        public async Task<IActionResult> AllRetirementPayment()
        {
            IEnumerable<RetirementPayment> res = null;
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
            if (role.Name == "ChiefAccountant" || role.Name == "Authorizer1" || role.Name == "Authorizer2" || role.Name == "AccountOfficer")
            {
                res = await retirementService.GetAllRetirementPayments(user.Id);
            }
            else
            {
                res = await retirementService.GetAllRetirementPaymentsForUser(user.Id);
            }
            
            foreach (var cash in res)
            {
                var cashUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == cash.UserId);
                cash.User = cashUser;
            }
            return View( new AllRetirementViewModel { retirementPayments = res.OrderByDescending(x => x.DateCreated).ToList() } );
        }

        [HttpGet]
        public async Task<IActionResult> ViewCashAdvanceToRetire()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var res = await retirementService.GetUnRetiredCashAdvanceForUser(user.Id);
                return View(res.OrderByDescending(x => x.DateCreated).ToList());
            }
            catch (Exception)
            {
                return RedirectToAction("Dashboard", "Voucher");
            }
        }

        [Authorize(Roles = "ChiefAccountant,Authorizer1,Authorizer2,AccountOfficer")]
        [HttpGet]
        public async Task<IActionResult> ViewApprovedRetirementPayment()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            if (user != null)
            {
                var res = await retirementService.GetApprovedRetirementPayment(user.Id);
                return View(res.OrderByDescending(x => x.DateCreated).ToList());
            }
            else
            {
                return RedirectToAction("Dashboard", "Voucher");
            }
        }

        [Authorize(Roles = "ChiefAccountant,Authorizer1,Authorizer2,AccountOfficer")]
        [HttpGet]
        public async Task<IActionResult> ViewUnApprovedRetirementPayment()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await retirementService.GetUnApprovedRetirementPayment(role.Id);
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
        public async Task<IActionResult> AddFiles(int retirementPaymentId, List<IFormFile> files)
        {
            string result;
            try
            {
                if (files == null)
                {
                    result = "1|Retirement payment updated successfully!";
                }
                else
                {
                    var user = await userManager.GetUserAsync(User);
                    var roles = await userManager.GetRolesAsync(user);
                    var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

                    var res = await retirementService.AddRetirementFiles(retirementPaymentId, files, user.Id, role.Id);
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
        public async Task<ViewResult> GetRetirePaymentFiles(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var retirementFiles = await retirementService.GetRetirePaymentFiles(Id);
            foreach (var file in retirementFiles)
            {
                file.RetirementPayment = await context.RetirementPayments.FirstOrDefaultAsync(x => x.Id == file.RetirementPaymentId);
            }
            return View(new RetirementFilesViewModel { retirementPaymentFiles = retirementFiles, UserId = user.Id });
        }

        public async Task<IActionResult> DeleteRetirementPaymentFile(int Id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await retirementService.DeleteRetirePaymentFile(Id, user.Id, role.Id);
                return RedirectToAction("GetRetirePaymentFiles", new { id = res.RetirementPaymentId });
            }
            catch (Exception ex)
            {
                var msg = ("0|" + ex.Message);
                return View("AllRetirementPayment");
            }
        }

        public async Task<IActionResult> DeleteRetirementPaymentRow(int Id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await retirementService.DeleteRetirePaymentCashBook(user.Id, role.Id, Id);
                return RedirectToAction("EditRetirementPayment", new { id = res.RetirePaymentId });
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
            var searchedRetirementPayments = new List<RetirementPayment>();
            var newRetirementPayment = await retirementService.GetAllRetirementPayments(user.Id);
            foreach (var retirementPayment in newRetirementPayment)
            {
                if (retirementPayment.DateCreated >= searchBydateViewModel.MinimumDate && retirementPayment.DateCreated <= searchBydateViewModel.MaximumDate)
                {
                    searchedRetirementPayments.Add(retirementPayment);
                }
            }
            //context.Entry(vouchers).State = EntityState.Detached;
            return View("AllRetirementPayment", searchedRetirementPayments.OrderByDescending(x => x.DateUpdated).ToList() );
        }

        public async Task<ActionResult> ViewRetirementPayment(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
            if (role != null)
            {
                var retirementPayment = await retirementService.GetRetirePayment(Id, user.Id);
                if (retirementPayment == null)
                {
                    return RedirectToAction("DashBoard", "Voucher");
                }
                var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == retirementPayment.DeptId);
                retirementPayment.Dept = dept;
                var payments = await retirementService.GetCashRetirePayment(Id);
                var actions = await retirementService.GetRetirePaymentAction(Id);
                var retirementPaymentFiles = await retirementService.GetRetirePaymentFiles(Id);
                foreach (var retirementPaymentFile in retirementPaymentFiles)
                {
                    retirementPaymentFile.RetirementPayment.UserId = retirementPayment.UserId;
                }
                foreach (var action in actions)
                {
                    action.User = await userManager.FindByIdAsync(action.UserId.ToString());
                    //action.User = await userManager.Users.FirstOrDefaultAsync(u => u.UserId == action.Id);
                }
                List<RetirementPaymentAction> ActionList = new List<RetirementPaymentAction>();
                foreach (var action in actions.OrderByDescending(x => x.DateUpdated))
                {

                    ActionList.Add(action);
                    if (action.ActionPerformed == ActionPerformed.Edited || action.ActionPerformed == ActionPerformed.Created)
                    {
                        break;
                    }

                }
                
                decimal? retireCash = System.Math.Abs(Convert.ToDecimal( retirementPayment.TotalRetirementAmount - retirementPayment.CashAdvanceAmount));
                string numberInWords = NumberToWords.ConvertAmount(Convert.ToDouble( retirementPayment.TotalRetirementAmount));
                foreach (var action in actions)
                {
                    if (action.UserId == user.Id)
                    {
                        return View("ViewRetirementPayment", new RetirementCompleteViewModel { retirementPayment = retirementPayment, retirementCashBookPayments = payments, retirementPaymentActions = ActionList.OrderBy(x => x.DateUpdated).ToList(), NumberInWords = numberInWords, retirementPaymentFiles = retirementPaymentFiles, retireCash = retireCash });
                    }
                }
                if (await userManager.IsInRoleAsync(user, "AccountOfficer"))
                {
                    return View("ViewRetirementPayment", new RetirementCompleteViewModel { retirementPayment = retirementPayment, retirementCashBookPayments = payments, retirementPaymentActions = ActionList.OrderBy(x => x.DateUpdated).ToList(), NumberInWords = numberInWords, retirementPaymentFiles = retirementPaymentFiles, retireCash = retireCash });
                }
                return RedirectToAction("DashBoard", "Voucher");

            }
            return View();
        }


        [HttpGet]
        public async Task<string> SendMail(RetirementPayment retirementPayment, List<ApplicationUser> users, string comment)
        {
            MailModel mailModel = new MailModel();
            var result = "";

            if (retirementPayment.IsApproved == false)
            {
                string msg = $"Dear Sir/Ma, you have a new cash retirement on your desk. Click to view https://bva.azurewebsites.net/RetirementPayment/RetirePaymentAction/{retirementPayment.Id}" +  " . Comment: " + comment;
                MailMessage mail = new MailMessage();
                foreach (var user in users)
                {
                    if (user.IsActive == true)
                    {
                        mail.To.Add(user.Email);
                    }
                }

                mail.From = new MailAddress("support@brandonetech.com" );
                mail.Subject = "NEW CASH RETIREMENT SLIP ON YOUR DESK";
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
            else if (retirementPayment.IsApproved == true)
            {
                string msg = $"This cash retirement has been approved, click to view. https://bva.azurewebsites.net/RetirementPayment/ViewRetirementPayment/{retirementPayment.Id}" +  " . Comment: " + comment;
                MailMessage mail = new MailMessage();
                foreach (var user in users)
                {
                    mail.To.Add(user.Email);
                }
                mail.From = new MailAddress("support@brandonetech.com" );
                mail.Subject = "CASH RETIREMENT APPROVED";
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
