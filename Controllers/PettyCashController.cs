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
    [Authorize]
    public class PettyCashController : Controller
    {
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IPettyCashService pettyService;
        private readonly AppDbContext context;

        public PettyCashController(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IPettyCashService pettyService, AppDbContext context)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.pettyService = pettyService;
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var userDept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            return View(new CreatePettyViewModel { User = user, Dept = userDept});
        }

        [HttpPost]
        public async Task<IActionResult> Create(PettyCashViewModel pettyCashViewModel)
        {
            string result;
            var user = await userManager.GetUserAsync(User);
            
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var res = await pettyService.AddNewPettyCash(user.Id, role.Id, pettyCashViewModel);
            if (res != null)
            {
                IEnumerable<ApplicationUser> mailUsers = await userManager.Users.Where(x => x.Id == res.UserToApprove).ToListAsync();
                await SendMail(res, mailUsers.ToList(), pettyCashViewModel.Comment);
                result = "1|" + res.Id + "|Petty Cash created successfully!";
            }
            else
            {
                result = "2|Error creating petty cash.";
            }
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> EditPettyCash(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            var pettyCash = await context.PettyCashes.FirstOrDefaultAsync(x => x.Id == Id);
            if (pettyCash.UserId == user.Id)
            {
                return View(new CompletePettyCashViewModel { PettyCash = pettyCash,  Dept = dept, User = user });
            }
            return RedirectToAction("PettyCashPending", "PettyCash");
        }

        [HttpPost]
        public async Task<IActionResult> EditPettyCash(PettyCash pettyCash, string comment)
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

                var res = await pettyService.EditPettyCash(pettyCash, user.Id, role.Id, comment);
                if (res != null)
                {
                    IEnumerable<ApplicationUser> mailUsers = await userManager.Users.Where(x => x.Id == res.UserToApprove).ToListAsync();
                    await SendMail(res, mailUsers.ToList(), comment);
                    result = "1|" + res.Id + "|Petty Cash updated successfully!";
                }
                else
                {
                    result = "2|Error updating Petty cash.";
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
        public async Task<ViewResult> PettyCashAction(int Id)
        {
            PettyCashApproval pettyCashApproval = null;
            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var pettyCash = await pettyService.GetPettyCash(Id, user.Id);
            var actions = await pettyService.GetPettyCashActions(Id);
            //add department
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == pettyCash.DeptId);

                pettyCashApproval = await context.PettyCashApprovals.FirstOrDefaultAsync(x => x.PettyCashID == pettyCash.Id);

            foreach (var action in actions)
            {
                action.User = await userManager.Users.FirstOrDefaultAsync(x => x.Id == action.UserId);
                //action.User = await userManager.Users.FirstOrDefaultAsync(u => u.Id == action.Id);
            }
            return View(new CompletePettyCashViewModel { PettyCash = pettyCash,  pettyCashActions = actions.OrderBy(x => x.DateUpdated).ToList(),  User = user, Dept = dept, CurrentUserId = user.Id, pettyCashApproval = pettyCashApproval });
        }

        [HttpPost]
        public async Task<IActionResult> PettyCashAction(CompletePettyCashViewModel pettyComplete)
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

                var res = await pettyService.PerformActionOnPettyCash(pettyComplete.Id, user.Id, role.Id, pettyComplete.comment, (ActionPerformed)pettyComplete.ActionPerformed);
                if (res != null)
                {
                    if (res.IsApproved == false)
                    {
                        IEnumerable<ApplicationUser> mailUsers = null;
                        var pettyCashApproval = await context.PettyCashApprovals.FirstOrDefaultAsync(x => x.PettyCashID == res.Id);
                        if (pettyComplete.ActionPerformed == 2)
                        {
                            mailUsers = await userManager.Users.Where(x => x.Id == pettyCashApproval.UserId).ToListAsync();
                        }
                        else
                        {
                            mailUsers = await userManager.Users.Where(x => x.Id == res.UserId).ToListAsync();
                        }
                        
                        await SendMail(res, mailUsers.ToList(), pettyComplete.comment);
                        if (pettyComplete.ActionPerformed == 2)
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
                        var users = await context.Users.ToListAsync();
                        foreach (var user1 in users)
                        {
                            if ((await userManager.IsInRoleAsync(user1, "ChiefAccountant"))|| (await userManager.IsInRoleAsync(user1, "AccountOfficer")))
                            {
                                mailUsers.Add(user1);
                            }
                        }
                        var actions = await context.pettyCashActions.Where(x => x.PettyCashId == res.Id).ToListAsync();
                        foreach (var action in actions)
                        {
                            mailUsers.Add(await context.Users.FirstOrDefaultAsync(x => x.Id == action.UserId));
                        }
                        await SendMail(res, mailUsers.ToList(), pettyComplete.comment);
                        if (pettyComplete.ActionPerformed == 2)
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
        public async Task<IActionResult> GetPettyCash(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var pettyCash = await pettyService.GetPettyCash(Id, user.Id);
            if (pettyCash == null)
            {
                throw new Exception("Petty Cash not found.");
            }

            return View(new CompletePettyCashViewModel { PettyCash = pettyCash});
        }

        [HttpGet]
        public async Task<IActionResult> PendingPettyCash()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            var res = await pettyService.GetPendingPettyCashForUser(role.Id, user.Id);
            return View(res.OrderByDescending(x => x.DateCreated));
        }

        [HttpGet]
        public async Task<IActionResult> AllPettyCash()
        {
            var user = await userManager.GetUserAsync(User);
            IEnumerable<PettyCash> res = null;
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            if (role.Name == "ChiefAccountant"|| role.Name == "Authorizer1" || role.Name == "Authorizer2" || role.Name == "AccountOfficer" || role.Name == "Approval" )
            {
                res = await pettyService.GetAllPettyCash(user.Id);
            }
            else
            {
                res = await pettyService.GetAllPettyCashForUser(user.Id);
            }
            

            List<PettyCashViewModel> pettyCash = new List<PettyCashViewModel>();
            foreach (var petty in res)
            {
                ApplicationUser approvalUser = null;
                var pettyUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == petty.UserId);
                if (petty.IsApproved == false)
                {
                    var approval = await context.PettyCashApprovals.FirstOrDefaultAsync(x => x.PettyCashID == petty.Id);
                    approvalUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == approval.UserId);
                    petty.User = pettyUser;

                    pettyCash.Add(new PettyCashViewModel
                    {
                        User = pettyUser,
                        TotalAmount = petty.TotalAmount,
                        Description = petty.Description,
                        Id = petty.Id,
                        ApprovalUser = approvalUser.FirstName + " " + approvalUser.LastName,
                        IsApproved = petty.IsApproved,
                        DateCreated = petty.DateCreated,
                        CurrentStage = petty.CurrentStage
                    });


                }
                else
                {
                    petty.User = pettyUser;

                    pettyCash.Add(new PettyCashViewModel
                    {
                        User = pettyUser,
                        TotalAmount = petty.TotalAmount,
                        Description = petty.Description,
                        Id = petty.Id,
                        IsApproved = petty.IsApproved,
                        DateCreated = petty.DateCreated,
                        CurrentStage = petty.CurrentStage
                    });
                }
            }
            return View(new AllPettyCashViewModel {pettyCashes = pettyCash.OrderByDescending(x => x.DateCreated).ToList() });
        }

        [Authorize(Roles = "ChiefAccountant,Authorizer1,Authorizer2,AccountOfficer")]
        [HttpGet]
        public async Task<IActionResult> ViewApprovedPettyCash()
        {
            var user = await userManager.GetUserAsync(User);
            if (user.IsActive == false)
            {
                await signInManager.SignOutAsync();
                return RedirectToAction("Login", "Account");
            }
            if (user != null)
            {
                var res = await pettyService.GetApprovedPettyCash(user.Id);
                return View(res.OrderByDescending(x => x.DateCreated).ToList());
            }
            else
            {
                return RedirectToAction("Dashboard", "Voucher");
            }
        }

        [Authorize(Roles = " ChiefAccountant,Authorizer1,Authorizer2,AccountOfficer")]
        [HttpGet]
        public async Task<IActionResult> ViewUnApprovedPettyCash()
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var res = await pettyService.GetUnApprovedPettyCash(user.Id);
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
        public async Task<IActionResult> AddFiles(int pettyCashId, List<IFormFile> files)
        {
            string result;
            try
            {
                if (files == null)
                {
                    result = "1|Petty Cash updated successfully!";
                }
                else
                {
                    var user = await userManager.GetUserAsync(User);
                    var roles = await userManager.GetRolesAsync(user);
                    var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

                    var res = await pettyService.AddPettyCashFiles(pettyCashId, files, user.Id, role.Id);
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
        public async Task<ViewResult> GetPettyCashFiles(int Id)
        {
            var user = await userManager.GetUserAsync(User);
            var pettyFiles = await pettyService.GetPettyCashFiles(Id);
            foreach (var file in pettyFiles)
            {
                file.Petty = await context.PettyCashes.FirstOrDefaultAsync(x => x.Id == file.PettyCashId);
            }
            return View(new PettyCashFilesViewModel { Files = pettyFiles, User = user });
        }

        public async Task<IActionResult> DeletePettyCashFile(int Id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await pettyService.DeletePettyCashFile(Id, user.Id, role.Id);
                return RedirectToAction("GetPettyCashFiles", new { id = res.PettyCashId });
            }
            catch (Exception ex)
            {
                var msg = ("0|" + ex.Message);
                return View("AllAdvancePayment");
            }
        }

        public async Task<IActionResult> DeletePettyCash(int Id)
        {
            try
            {
                var user = await userManager.GetUserAsync(User);
                var roles = await userManager.GetRolesAsync(user);
                var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                var res = await pettyService.DeletePettyCashFile(user.Id, role.Id, Id);
                return RedirectToAction("EditPettyCash", new { id = res.PettyCashId });
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
            var newPettyCash = await pettyService.GetAllPettyCash(user.Id);
            List<PettyCashViewModel> pettyCash = new List<PettyCashViewModel>();
            foreach (var petty in newPettyCash)
            {
                if (petty.DateCreated >= searchBydateViewModel.MinimumDate && petty.DateCreated <= searchBydateViewModel.MaximumDate)
                {
                    ApplicationUser approvalUser = null;
                    var pettyUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == petty.UserId);
                    if (petty.IsApproved == false)
                    {
                        var approval = await context.PettyCashApprovals.FirstOrDefaultAsync(x => x.PettyCashID == petty.Id);
                        approvalUser = await userManager.Users.FirstOrDefaultAsync(x => x.Id == approval.UserId);
                    }
                    petty.User = pettyUser;

                    pettyCash.Add(new PettyCashViewModel
                    {
                        User = pettyUser,
                        TotalAmount = petty.TotalAmount,
                        Description = petty.Description,
                        Id = petty.Id,
                        ApprovalUser = approvalUser.FirstName + " " + approvalUser.LastName,
                        IsApproved = petty.IsApproved,
                        DateCreated = petty.DateCreated
                    });
                }
            }
            
            return View("AllPettyCash", new AllPettyCashViewModel { pettyCashes = pettyCash.OrderByDescending(x => x.DateCreated).ToList() });
        }

        public async Task<ActionResult> ViewPettyCash(int Id)
        {
            List<PettyCashAction> ActionList = new List<PettyCashAction>();

            var user = await userManager.GetUserAsync(User);
            var roles = await userManager.GetRolesAsync(user);
            var role = await roleManager.FindByNameAsync(roles.SingleOrDefault());

            if (role != null)
            {
                var pettyCash = await pettyService.GetPettyCash(Id, user.Id);
                pettyCash.User = await context.Users.FindAsync(pettyCash.UserId);

                var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == pettyCash.DeptId);

                //cashAdvance.Dept = dept;
                var actions = await pettyService.GetPettyCashActions(Id);
                var pettyCashFiles = await pettyService.GetPettyCashFiles(Id);
                pettyCash.Dept = dept;

                string numberInWords = NumberToWords.ConvertAmount(Convert.ToDouble(pettyCash.TotalAmount.Value));

                foreach (var action in actions)
                {
                    action.User = await userManager.FindByIdAsync(action.UserId.ToString());
                    //action.User = await userManager.Users.FirstOrDefaultAsync(u => u.UserId == action.Id);
                }

                if (pettyCash == null)
                {
                    return View("ViewPettyCash", new CompletePettyCashViewModel { PettyCash = pettyCash, pettyCashActions = ActionList.OrderBy(x => x.DateUpdated).ToList(), NumberInWords = numberInWords, pettyCashFiles = pettyCashFiles });

                }

                foreach (var action in actions.OrderByDescending(x => x.DateUpdated))
                {
                    ActionList.Add(action);
                    if (action.ActionPerformed == ActionPerformed.Edited || action.ActionPerformed == ActionPerformed.Created)
                    {
                        break;
                    }
                }

                foreach (var action in actions)
                {
                    if (action.UserId == user.Id)
                    {
                        return View("ViewPettyCash", new CompletePettyCashViewModel { PettyCash = pettyCash, pettyCashActions = ActionList.OrderBy(x => x.DateUpdated).ToList(), NumberInWords = numberInWords, pettyCashFiles = pettyCashFiles });
                    }
                }

                if (await userManager.IsInRoleAsync(user, "AccountOfficer"))
                {
                    return View("ViewPettyCash", new CompletePettyCashViewModel { PettyCash = pettyCash, pettyCashActions = ActionList.OrderBy(x => x.DateUpdated).ToList(), NumberInWords = numberInWords, pettyCashFiles = pettyCashFiles });
                }
            }

            return View();
        }



        [HttpGet]
        public async Task<string> SendMail(PettyCash pettyCash, List<ApplicationUser> users, string comment)
        {
            MailModel mailModel = new MailModel();
            var result = "";

            if (pettyCash.IsApproved == false)
            {
                string msg = $"Dear Sir/Ma, you have a new petty cash on your desk. Click to view https://bva.azurewebsites.net/PettyCash/PettyCashAction/{pettyCash.Id}"  + " . Comment: " + comment;
                MailMessage mail = new MailMessage();
                foreach (var user in users)
                {
                    if (user.IsActive == true)
                    {
                        mail.To.Add(user.Email);
                    }
                }

                mail.From = new MailAddress("support@brandonetech.com" );
                mail.Subject = "NEW PETTY CASH SLIP ON YOUR DESK";
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
            else if (pettyCash.IsApproved == true)
            {
                string msg = $"This Petty Cash has been approved, click to view. https://bva.azurewebsites.net/PettyCash/ViewPettyCash/{pettyCash.Id}"  + " . Comment: " + comment;
                MailMessage mail = new MailMessage();
                foreach (var user in users)
                {
                    mail.To.Add(user.Email);
                }
                mail.From = new MailAddress("support@brandonetech.com");
                mail.Subject = "PETTY CASH APPROVED";
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
