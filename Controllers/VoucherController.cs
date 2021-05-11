using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
//using Rotativa;
using Rotativa.AspNetCore;
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
    [Authorize]
    public class VoucherController : Controller
    {
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IApplicationService voucherService;
        private readonly AppDbContext context;

        public VoucherController(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IApplicationService voucherService, AppDbContext context)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.voucherService = voucherService;
            this.context = context;
        }

        [Authorize(Roles = "AccountOfficer, ChiefAccountant")]
        [HttpGet]
        public ViewResult Create()
        {
            var particulars = context.Particulars;
            return View(particulars);
        }
        [Authorize(Roles = "AccountOfficer, ChiefAccountant")]
        [HttpPost]
        public async Task<IActionResult> Create(VoucherViewModel voucher, List<CashBookViewModel> cashBookViewModels /*VoucherModel voucherModel*/)
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

            var res = await voucherService.AddNewVoucher(user.Id, role.Id, voucher, cashBookViewModels);
            //var file = await AddFiles(res.Id, uploadfiles);
            if (res != null)
            {
                var mailUsers = await userManager.GetUsersInRoleAsync(res.CurrentLevelRoleName);
                SendMail(res, mailUsers.ToList(), voucher.Comment);
                result = "1|" + res.Id + "|Voucher created successfully!";
            }
            else
            {
                result = "2|Error creating voucher.";
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<ViewResult> GetVoucher(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var voucher = await voucherService.GetVoucher(Id, role.Name);
            var cashBooks = await voucherService.GetCashbook(Id);
            if (voucher == null)
            {
                throw new Exception("Voucher not found.");
            }

            return View(new VoucherCashBookViewModel { Voucher = voucher, CashBooks = cashBooks });
        }


        [HttpGet]
        public async Task<ViewResult> VoucherAction(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var voucher = await voucherService.GetVoucher(Id, role.Name);
            var cashBooks = await voucherService.GetCashbook(Id);
            var actions = await voucherService.GetVoucherActions(Id);
            foreach (var action in actions)
            {
                action.User = await userManager.Users.FirstOrDefaultAsync(x => x.Id == action.UserId);
                //action.User = await userManager.Users.FirstOrDefaultAsync(u => u.Id == action.Id);
            }
            return View(new VoucherCashBookViewModel { Voucher = voucher, CashBooks = cashBooks, Actions = actions.OrderBy(x => x.DateUpdated) });
        }

        [Authorize(Roles = "AccountOfficer, ChiefAccountant")]
        [HttpGet]
        public async Task<IActionResult> EditVoucher(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var particulars = context.Particulars.ToList(); ;
            var voucher = await voucherService.GetVoucher(Id, role.Name);
            var cashBooks = await voucherService.GetCashbook(Id);
            var actions = await voucherService.GetVoucherActions(Id);
            if ((voucher.CurrentLevelRoleName == "ChiefAccountant" || voucher.CurrentLevelRoleName == "AccountOfficer") || voucher.CurrentLevelRoleName == "Authorizer1" || voucher.CurrentLevelRoleName == "Approval")
            {
                return View(new VoucherCashBookViewModel { Particulars = particulars, Voucher = voucher, CashBooks = cashBooks, Actions = actions.OrderBy(x => x.DateUpdated) });
            }
            return RedirectToAction("voucherspending", "voucher");
        }

        [Authorize(Roles = "AccountOfficer, ChiefAccountant")]
        [HttpPost]
        public async Task<IActionResult> EditVoucher(Voucher voucher, List<CashBook> cashBook, CommentModel comment)
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

                var res = await voucherService.EditVoucher(voucher, cashBook, user.Id, role.Id, comment.Comment);
                if (res != null)
                {
                    var mailUsers = await userManager.GetUsersInRoleAsync(res.CurrentLevelRoleName);
                    SendMail(res, mailUsers.ToList(), comment.Comment);
                    result = "1|" + res.Id + "|Voucher updated successfully!";
                }
                else
                {
                    result = "2|Error updating voucher.";
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
        public async Task<IActionResult> VouchersPending()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var res = await voucherService.GetVouchersForRole(role.Id, user.Id);
            return View(res.OrderByDescending(x => x.DateCreated));
        }

        [HttpGet]
        public async Task<IActionResult> ViewAllVouchers()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var res = await voucherService.GetVouchersForRole(role.Id, user.Id);
            var vouchers = await voucherService.GetAllVouchers(role.Name);
            foreach (var voucher in vouchers)
            {
                if (voucher.IsActive == false)
                {
                    if (voucher.CurrentLevelRoleName == "Approval")
                    {
                        var approval = await context.ApprovalVouchers.FirstOrDefaultAsync(x => x.VoucherId == voucher.Id && x.IsActive == false);
                        
                        if (approval != null)
                        {
                            var approvalUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == approval.UserId);
                            voucher.CurrentLevelRoleName = approvalUser.FirstName + " " + approvalUser.LastName;
                        }
                    }
                    else
                    {
                        var voucherUsers = await userManager.GetUsersInRoleAsync(voucher.CurrentLevelRoleName);
                        var voucherUser = voucherUsers.FirstOrDefault(x => x.IsActive == true);
                        voucher.CurrentLevelRoleName = voucherUser.FirstName +" "+ voucherUser.LastName;
                    }
                }
            }
            int totalPendingVouchers = res.Count();
            return View(new ViewAllVouchersModel { vouchers = vouchers.OrderByDescending(x => x.DateCreated).ToList(), TotalPendingVouchers = totalPendingVouchers });
        }

        [HttpGet]
        public async Task<IActionResult> ViewActiveVouchers()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
            if (role != null)
            {
                var res = await voucherService.GetActiveVouchers(role.Name);
                return View(res.OrderByDescending(x => x.DateCreated).ToList());
            }
            else
            {
                return View("Dashboard");
            }

        }

        [Authorize(Roles = "AccountOfficer, ChiefAccountant")]
        [HttpGet]
        public async Task<IActionResult> ViewInActiveVouchers()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await voucherService.GetInActiveVouchers(role.Id);
                return View(res.OrderByDescending(x => x.DateCreated).ToList());
            }
            catch (Exception)
            {
                return RedirectToAction("Dashboard");
            }

        }
        [HttpPost]
        public async Task<IActionResult> VoucherAction(VoucherCashBookViewModel voucherCashBookViewModel)
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

                var res = await voucherService.PerformActionOnVoucher(voucherCashBookViewModel.Id, user.Id, role.Id,
                    voucherCashBookViewModel.Comment, (ActionPerformed)voucherCashBookViewModel.ActionPerformed);
                if (res != null)
                {
                    if (res.IsActive == false)
                    {
                        var mailUsers = await userManager.GetUsersInRoleAsync(res.CurrentLevelRoleName);
                        await SendMail(res, mailUsers.ToList(), voucherCashBookViewModel.Comment);
                        if (voucherCashBookViewModel.ActionPerformed == 2)
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
                        List<ApplicationUser> mailUsers = new List<ApplicationUser>();
                        var allUsers = await userManager.Users.ToListAsync();
                        if (res.TotalAmount > 50000)
                        {
                            if (res.RoleCreator == "AccountOfficer")
                            {
                                foreach (var mailUser in allUsers)
                                {
                                    var userRole = await userManager.GetRolesAsync(mailUser);
                                    if (userRole != null)
                                    {
                                        var roleDetail = await roleManager.FindByNameAsync(userRole.SingleOrDefault());
                                        if (roleDetail.Name != "Admin" || roleDetail.Name != "Approval")
                                        {
                                            mailUsers.Add(mailUser);
                                        }
                                    }
                                }
                            }
                            else if (res.RoleCreator == "ChiefAccountant")
                            {
                                foreach (var mailUser in allUsers)
                                {
                                    var userRole = await userManager.GetRolesAsync(mailUser);
                                    if (userRole != null)
                                    {
                                        var roleDetail = await roleManager.FindByNameAsync(userRole.SingleOrDefault());
                                        if (roleDetail.Name != "Admin" || roleDetail.Name != "Approval" || roleDetail.Name != "AccountOfficer")
                                        {
                                            mailUsers.Add(mailUser);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (var mailUser in allUsers)
                            {
                                var userRole = await userManager.GetRolesAsync(mailUser);
                                if (userRole != null)
                                {
                                    var roleDetail = await roleManager.FindByNameAsync(userRole.SingleOrDefault());
                                    if (roleDetail.Name != "Admin" )
                                    {
                                        mailUsers.Add(mailUser);
                                    }
                                }
                            }
                        }
                        
                        //context.Entry(allUsers).State = EntityState.Detached;
                        await SendMail(res, mailUsers, voucherCashBookViewModel.Comment);
                        if (voucherCashBookViewModel.ActionPerformed == 2)
                        {
                            result = "1|Approval successfull!";
                        }
                        else
                        {
                            result = "1|Rejection successfull!";
                        }
                    }
                }
                else
                {
                    result = "0|Action was not carried out successfully.";
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
        public async Task<ViewResult> GetVoucherFiles(int Id)
        {
            var voucherFiles = await voucherService.GetVoucherFiles(Id);
            foreach (var voucherFile in voucherFiles)
            {
                var voucher = await context.Vouchers.FindAsync(voucherFile.VoucherId);
                voucherFile.Voucher.CurrentLevelRoleName = voucher.CurrentLevelRoleName;
            }

            return View(voucherFiles);
        }

        [Authorize(Roles = "AccountOfficer, ChiefAccountant")]
        public async Task<IActionResult> DeleteVoucherFile(int Id)
        {
            try
            {
                string result;
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await voucherService.DeleteVoucherFiles(Id, user.Id, role.Id);
                return RedirectToAction("GetVoucherFiles", new { id = res.VoucherId });
            }
            catch (Exception ex)
            {
                var msg = ("0|" + ex.Message);
                return View("ViewAllVouchers");
            }

        }

        [Authorize(Roles = "AccountOfficer, ChiefAccountant")]
        public async Task<IActionResult> DeleteCashbook(int Id)
        {
            try
            {
                string result;
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await voucherService.DeleteCashBook(Id, user.Id, role.Id);
                return RedirectToAction("EditVoucher", new { id = res.VoucherId });
            }
            catch (Exception ex)
            {
                var msg = "2|" + ex.Message;
                return Json(msg);
            }

        }

        [Authorize(Roles = "AccountOfficer, ChiefAccountant")]
        [HttpGet]
        public ViewResult AddFiles(int Id)
        {
            return View();
        }

        [Authorize(Roles = "AccountOfficer, ChiefAccountant")]
        [HttpPost]
        public async Task<IActionResult> AddFiles(int voucherId, List<IFormFile> files)
        {
            string result;
            try
            {
                if (files == null)
                {
                    result = "1|Voucher updated successfully!";
                }
                else
                {
                    var user = await userManager.GetUserAsync(User);
                    var roles = await userManager.GetRolesAsync(user);
                    var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

                    var res = await voucherService.AddVoucherFiles(voucherId, files, user.Id, role.Id);
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

        //[HttpPost]
        //public async Task<JsonResult> EditFiles(int voucherId, VoucherFileViewModel voucherFileViewModels)
        //{
        //    try
        //    {
        //        string result;
        //        var user = await userManager.GetUserAsync(User);
        //        var roles = await userManager.GetRolesAsync(user);
        //        var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

        //        var res = await voucherService.EditVoucherFiles(voucherId, voucherFileViewModels, user.Id, role.Id);
        //        if (res != null)
        //        {
        //            result = "1|Voucher files successfully!";
        //        }
        //        else
        //        {
        //            result = "2|Error creating voucher files.";
        //        }
        //        return Json(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        var msg = ("2|" + ex.Message);
        //        return Json(msg);
        //    }

        //}

        [HttpGet]
        public async Task<string> SendMail(Voucher voucher, List<ApplicationUser> users,string comment)
        {
            MailModel mailModel = new MailModel();
            var result = "";

            if (voucher.IsActive == false)
            {
                string msg = "Dear Sir/Ma, you have a new voucher on your desk. Click to view https://bva.azurewebsites.net/voucher/voucheraction/" + voucher.Id + ". Comment: " + comment;
                MailMessage mail = new MailMessage();

                if (voucher.CurrentLevelRoleName == "Approval")
                {
                    var approvalVouch =  context.ApprovalVouchers.FirstOrDefault(x => x.VoucherId == voucher.Id);
                    var approvalUser = context.Users.Find(approvalVouch.UserId);
                    mail.To.Add(approvalUser.Email);
                }
                else
                {
                    foreach (var user in users)
                    {
                        if (user.IsActive == true)
                        {
                            mail.To.Add(user.Email);
                        }
                    }
                }
                
                mail.From = new MailAddress("support@brandonetech.com");
                mail.Subject = "New Voucher";
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
                //return result;
            }
            else if (voucher.IsActive == true)
            {
                string msg = $"This voucher has been approved, click to view. https://bva.azurewebsites.net/voucher/viewvoucher/" + voucher.Id + ". Comment: " + comment;
                MailMessage mail = new MailMessage();
                foreach (var user in users)
                {
                        mail.To.Add(user.Email);
                }
                mail.From = new MailAddress("support@brandonetech.com");
                mail.Subject = "Voucher Approved";
                //string Body = mailModel.Body;
                mail.Body = msg;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtppro.zoho.com";
                smtp.Port = 587;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("suppor@brandonetech.com", "*123*brandonetech#"); // Enter senders User name and password       
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
            return result;
        }
        public async Task<ActionResult> ViewVoucher(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
            if (role != null)
            {
                var voucher = await voucherService.GetVoucher(Id, role.Name);
                if (voucher == null)
                {
                    return RedirectToAction("DashBoard");
                }
                var cashBooks = await voucherService.GetCashbook(Id);
                var actions = await voucherService.GetVoucherActions(Id);
                var voucherFiles = await voucherService.GetVoucherFiles(Id);
                foreach (var voucherFile in voucherFiles)
                {
                    voucherFile.Voucher.CurrentLevelRoleName = voucher.CurrentLevelRoleName;
                }
                foreach (var action in actions)
                {
                    action.User = await userManager.FindByIdAsync(action.UserId.ToString());
                    //action.User = await userManager.Users.FirstOrDefaultAsync(u => u.UserId == action.Id);
                }
                List<Models.Action> ActionList = new List<Models.Action>();
                foreach (var action in actions.OrderByDescending(x => x.DateUpdated))
                {

                    ActionList.Add(action);
                    if (action.ActionPerformed == ActionPerformed.Edited || action.ActionPerformed == ActionPerformed.Created)
                    {
                        break;
                    }

                }
                string numberInWords = NumberToWords.ConvertAmount(voucher.TotalAmount.Value);
                return View("ViewVoucher", new VoucherCashBookViewModel { Voucher = voucher, CashBooks = cashBooks, Actions = ActionList.OrderBy(x => x.DateUpdated), NumberInWords = numberInWords, VoucherFiles = voucherFiles });
                //return new ViewAsPdf("ViewVoucher", new VoucherCashBookViewModel { Voucher = voucher, CashBooks = cashBooks, Actions = ActionList.OrderBy(x => x.DateUpdated), NumberInWords = numberInWords })
                //{
                //    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                //    FileName = ViewData[voucher.VoucherType.ToString() + voucher.Id].ToString()
                //};
            }
            return View();
        }
        public async Task<ActionResult> ViewVoucherPdf(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
            if (role != null)
            {
                var voucher = await voucherService.GetVoucher(Id, role.Name);
                var cashBooks = await voucherService.GetCashbook(Id);
                var actions = await voucherService.GetVoucherActions(Id);
                var voucherFiles = await voucherService.GetVoucherFiles(Id);
                foreach (var voucherFile in voucherFiles)
                {
                    voucherFile.Voucher.CurrentLevelRoleName = voucher.CurrentLevelRoleName;
                }
                foreach (var action in actions)
                {
                    action.User = await userManager.FindByIdAsync(action.UserId.ToString());
                    //action.User = await userManager.Users.FirstOrDefaultAsync(u => u.UserId == action.Id);
                }
                List<Models.Action> ActionList = new List<Models.Action>();
                foreach (var action in actions.OrderByDescending(x => x.DateUpdated))
                {

                    ActionList.Add(action);
                    if (action.ActionPerformed == ActionPerformed.Edited || action.ActionPerformed == ActionPerformed.Created)
                    {
                        break;
                    }

                }
                string numberInWords = NumberToWords.ConvertAmount(voucher.TotalAmount.Value);
                //return View("ViewVoucherPdf", new VoucherCashBookViewModel { Voucher = voucher, CashBooks = cashBooks, Actions = ActionList.OrderBy(x => x.DateUpdated), NumberInWords = numberInWords, VoucherFiles = voucherFiles });
                return new ViewAsPdf("ViewVoucherPdf", new VoucherCashBookViewModel { Voucher = voucher, CashBooks = cashBooks, Actions = ActionList.OrderBy(x => x.DateUpdated), NumberInWords = numberInWords, VoucherFiles = voucherFiles })
                {
                    PageSize = Rotativa.AspNetCore.Options.Size.A4,
                    //FileName = ViewData[voucher.VoucherType.ToString() + voucher.Id].ToString()
                };
            }
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> DashBoard()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
            var dashBoard = await voucherService.DashBoard();
            //IEnumerable<Voucher> res;
            foreach (var rolee in roles)
            {
                var rol = await roleManager.FindByNameAsync(rolee);
                var res = await voucherService.GetVouchersForRole(rol.Id, user.Id);
                dashBoard.PendingCount = res.Count();
            }

            var allVouchers = await voucherService.GetAllVouchers(role.Name);
            dashBoard.TotalVouchers = allVouchers.Count();
            return View(dashBoard);
        }

        [HttpPost]
        public async Task<IActionResult> SearchByDate(SearchBydateViewModel searchBydateViewModel)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            if (searchBydateViewModel.MinimumDate > searchBydateViewModel.MaximumDate)
                throw new ArgumentException("From Date cannot be greater than To Date");
            var searchedVouchers = new List<Voucher>();
            var newVouchers = await voucherService.GetAllVouchers(role.Name);
            foreach (var voucher in newVouchers)
            {
                if (voucher.DateCreated >= searchBydateViewModel.MinimumDate && voucher.DateCreated <= searchBydateViewModel.MaximumDate)
                {
                    searchedVouchers.Add(voucher);
                }
            }
            //context.Entry(vouchers).State = EntityState.Detached;
            return View("ViewAllVouchers", new ViewAllVouchersModel { vouchers = searchedVouchers.OrderByDescending(x => x.DateUpdated).ToList() });
        }

    }

}
