using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Controllers;
using VoucherAutomationSystem.Models;
using VoucherAutomationSystem.Models.Enums;
using VoucherAutomationSystem.ViewModels;

namespace VoucherAutomationSystem.Data
{
    public class RetirementPaymentService : IRetirementPaymentService
    {
        private readonly AppDbContext context;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHostingEnvironment hostingEnvironment;

        public RetirementPaymentService(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, AppDbContext context,
            IHostingEnvironment hostingEnvironment)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.context = context;
            this.hostingEnvironment = hostingEnvironment;
        }
        public async Task<RetirementPayment> AddRetirementFiles(int retirementPaymentId, List<IFormFile> files, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var retirementPayment = await context.RetirementPayments.FindAsync(retirementPaymentId);
            if (retirementPayment == null)
                throw new Exception("Cash Advance not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            foreach (var file in files)
            {
                var fileUrl = SavePhoto(file);
                await context.AddAsync(new RetirementPaymentFile
                {
                    Name = fileUrl,
                    RetirementPaymentId = retirementPaymentId,
                    RetirementPayment = retirementPayment
                });
            }
            await context.SaveChangesAsync();
            return retirementPayment;
        }

        public async Task<RetirementPayment> AddRetirementPayment(int UserId, int RoleId, RetirementPaymentViewModel retirementPaymentViewModel, List<RetirementCashPaymentsViewModel> retirementCashPaymentsViewModels, int cashAdvanceId)
        {
            var retire = await context.RetirementPayments.FirstOrDefaultAsync(x => x.CashAdvanceId == cashAdvanceId);
            if (retire != null)
                throw new Exception("Retirement Payment already created.");
            //Get user Id 
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");
            var cashAdvance = await context.CashAdvances.FirstOrDefaultAsync(x => x.Id == cashAdvanceId);
            if (cashAdvance == null)
                throw new Exception("Cash Advance not found.");
            //Get user Role
            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");
            //Get department
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            if (dept == null)
                throw new Exception("User not in any department.");
            //Add the new cash advance to db
            var retirementPayment = new RetirementPayment
            {
                Name = cashAdvance.Name,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                IsApproved = false,
                CurrentStage = 2,
                CashAdvanceId = cashAdvance.Id,
                ExchangeRate = cashAdvance.ExchangeRate,
                DeptId = cashAdvance.DeptId,
                Description = retirementPaymentViewModel.Description,
                Currency = cashAdvance.Currency,
                CashAdvanceAmount = cashAdvance.TotalAmount,
                TotalRetirementAmount = retirementPaymentViewModel.TotalRetirementAmount,
                RoleId = role.Id,
                UserId = user.Id,
                User = user,
                Dept = dept
            };
            
            await context.AddAsync(retirementPayment);

            await context.SaveChangesAsync();
            //Add list of Cash advance payment and get total amount
            decimal totalAmount = 0;
            foreach (var retirementCashPaymentsView in retirementCashPaymentsViewModels)
            {
                await context.AddAsync(new RetirementCashBookPayments
                {
                    Amount = retirementCashPaymentsView.Amount,
                    Description = retirementCashPaymentsView.Description,
                    RetirePaymentId = retirementPayment.Id,
                    RetirementPayment = retirementPayment
                });
                totalAmount += retirementCashPaymentsView.Amount;
            }
            //Add new advance payment action
            context.Add(new RetirementPaymentAction
            {
                ActionPerformed = Models.Enums.ActionPerformed.Created,
                Comment = retirementPaymentViewModel.Comment,
                UserId = UserId,
                RetirementPaymentId = retirementPayment.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });
            //Total amount
            retirementPayment.TotalRetirementAmount = totalAmount;
            //set isCredit
            var amountDifference = cashAdvance.TotalAmount - totalAmount;

            if (amountDifference > 0)
            {
                retirementPayment.IsCredit = true;
            }
            else
            {
                retirementPayment.IsCredit = false;
            }
            //set current stage 
            if (dept.Name == GeneralClass.Account && user.RoleLead == false)
            {
                retirementPayment.CurrentStage = 3;
            }
            if (user.RoleLead == true && role.Name != GeneralClass.ChiefAccountant)
            {
                retirementPayment.CurrentStage = 3;
            }
            else if (user.RoleLead == true && role.Name == GeneralClass.ChiefAccountant)
            {
                retirementPayment.CurrentStage = 4;
            }
            context.Update(retirementPayment);
            await context.SaveChangesAsync();

            return retirementPayment;
        }

        public async Task<RetirementPayment> CreateRetirementPayment(int UserId, int RoleId, RetirementPaymentViewModel retirementPaymentViewModel, List<RetirementCashPaymentsViewModel> retirementCashPaymentsViewModels, int cashAdvanceId)
        {

            //Get user Id 
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");
            //Get user Role
            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");
            //Get department
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            if (dept == null)
                throw new Exception("User not in any department.");
            //Add the new cash advance to db
            var retirementPayment = new RetirementPayment
            {
                Name = user.FirstName + " " + user.LastName,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                IsApproved = false,
                CurrentStage = 2,
                CashAdvanceId = 0,
                ExchangeRate = retirementPaymentViewModel.ExchangeRate,
                DeptId = retirementPaymentViewModel.DeptId,
                Description = retirementPaymentViewModel.Description,
                Currency = retirementPaymentViewModel.Currency,
                CashAdvanceAmount = retirementPaymentViewModel.CashAdvanceAmount,
                TotalRetirementAmount = retirementPaymentViewModel.TotalRetirementAmount,
                RoleId = role.Id,
                UserId = user.Id,
                User = user,
                Dept = dept
            };

            await context.AddAsync(retirementPayment);

            await context.SaveChangesAsync();
            //Add list of Cash advance payment and get total amount
            decimal totalAmount = 0;
            foreach (var retirementCashPaymentsView in retirementCashPaymentsViewModels)
            {
                await context.AddAsync(new RetirementCashBookPayments
                {
                    Amount = retirementCashPaymentsView.Amount,
                    Description = retirementCashPaymentsView.Description,
                    RetirePaymentId = retirementPayment.Id,
                    RetirementPayment = retirementPayment
                });
                totalAmount += retirementCashPaymentsView.Amount;
            }
            //Add new advance payment action
            context.Add(new RetirementPaymentAction
            {
                ActionPerformed = Models.Enums.ActionPerformed.Created,
                Comment = retirementPaymentViewModel.Comment,
                UserId = UserId,
                RetirementPaymentId = retirementPayment.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });
            //Total amount
            retirementPayment.TotalRetirementAmount = totalAmount;
            //set isCredit
            var amountDifference = retirementPaymentViewModel.CashAdvanceAmount - totalAmount;

            if (amountDifference > 0)
            {
                retirementPayment.IsCredit = true;
            }
            else
            {
                retirementPayment.IsCredit = false;
            }
            //set current stage 
            if (dept.Name == GeneralClass.Account && user.RoleLead == false)
            {
                retirementPayment.CurrentStage = 3;
            }
            if (user.RoleLead == true && role.Name != GeneralClass.ChiefAccountant)
            {
                retirementPayment.CurrentStage = 3;
            }
            else if (user.RoleLead == true && role.Name == GeneralClass.ChiefAccountant)
            {
                retirementPayment.CurrentStage = 4;
            }
            context.Update(retirementPayment);
            await context.SaveChangesAsync();

            return retirementPayment;
        }

        public async Task<RetirementCashBookPayments> DeleteRetirePaymentCashBook(int UserId, int RoleId, int retirementPaymentId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var retirePaymentCashBook = await context.RetirementCashBookPayments.FindAsync(retirementPaymentId);
            var retirePayment = await context.RetirementPayments.FindAsync(retirePaymentCashBook.RetirePaymentId);
            if (retirePayment.UserId != UserId)
                throw new Exception("Retire Payment Payment cannot be deleted by this user.");
            if (retirePaymentCashBook == null)
                throw new Exception("Retire Payment Payment not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            context.RetirementCashBookPayments.Remove(retirePaymentCashBook);
            await context.SaveChangesAsync();
            return retirePaymentCashBook;
        }

        public async Task<RetirementPaymentFile> DeleteRetirePaymentFile(int retirementPaymentFileId, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var retirePaymentFile = await context.RetirementPaymentFiles.FindAsync(retirementPaymentFileId);
            var retirePayment = await context.RetirementPayments.FindAsync(retirePaymentFile.RetirementPaymentId);
            if (retirePayment.UserId != UserId)
                throw new Exception("Retirement Payment file cannot be deleted by this user.");
            if (retirePayment.IsApproved == true)
                throw new Exception("Retirement Payment is approved, cant be deleted!");
            if (retirePaymentFile == null)
                throw new Exception("Retirement Payment File not found.");

            context.RetirementPaymentFiles.Remove(retirePaymentFile);
            await context.SaveChangesAsync();
            return retirePaymentFile;
        }

        public async Task<RetirementPayment> EditRetirePayment(RetirementPayment retirementPayment, List<RetirementCashBookPayments> retirementCashBookPayments, int UserId, int RoleId, string comment)
        {
            var newRetirementPayment = await context.RetirementPayments.FindAsync(retirementPayment.Id);
            newRetirementPayment.ExchangeRate = retirementPayment.ExchangeRate;
            newRetirementPayment.Description = retirementPayment.Description;
            newRetirementPayment.Currency = retirementPayment.Currency;

            context.Entry(retirementPayment).State = EntityState.Detached;
            //var cashAdvance = await context.CashAdvances.FirstOrDefaultAsync(x => x.Id == newRetirementPayment.CashAdvanceId);
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            if (newRetirementPayment.IsApproved == true)
                throw new Exception("Action failed, Retirement money is approved");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            if (UserId != newRetirementPayment.UserId)
                throw new Exception("This user does not have permission to edit this retirement money.");

            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            if (dept == null)
                throw new Exception("This user does not have permission to edit this retirement money.");

            newRetirementPayment.CurrentStage = 2;
            context.Update(newRetirementPayment);
            
            IEnumerable<RetirementCashBookPayments> oldRetirementMoney = context.RetirementCashBookPayments.Where(x => x.RetirePaymentId == newRetirementPayment.Id);

            foreach (var retirementCashBook in oldRetirementMoney)
            {
                context.RetirementCashBookPayments.Remove(retirementCashBook);
            }
            // Add the CashBook
            decimal totalAmount = 0;
            foreach (var retirementCash in retirementCashBookPayments)
            {
                await context.AddAsync(new RetirementCashBookPayments
                {
                    Amount = retirementCash.Amount,
                    Description = retirementCash.Description,
                    RetirePaymentId = newRetirementPayment.Id
                });
                totalAmount += retirementCash.Amount;
            }
            
            //Add new advance payment action
            context.Add(new RetirementPaymentAction
            {
                ActionPerformed = Models.Enums.ActionPerformed.Edited,
                Comment = comment,
                UserId = UserId,
                RetirementPaymentId = newRetirementPayment.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });

            newRetirementPayment.DateUpdated = DateTime.Now;
            newRetirementPayment.TotalRetirementAmount = totalAmount;
            var amountDifference = newRetirementPayment.CashAdvanceAmount - totalAmount;
            
            if (amountDifference > 0)
            {
                newRetirementPayment.IsCredit = true;
            }
            else
            {
                newRetirementPayment.IsCredit = false;
            }
            //set current stage 
            if (dept.Name == GeneralClass.Account && user.RoleLead == false)
            {
                newRetirementPayment.CurrentStage = 3;
            }
            if (user.RoleLead == true && role.Name != GeneralClass.ChiefAccountant)
            {
                newRetirementPayment.CurrentStage = 3;
            }
            else if (user.RoleLead == true && role.Name == GeneralClass.ChiefAccountant)
            {
                newRetirementPayment.CurrentStage = 4;
            }
            context.Update(newRetirementPayment);
            await context.SaveChangesAsync();

            return newRetirementPayment;
        }

        public async Task<IEnumerable<RetirementPayment>> GetAllRetirementPayments(int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");
            return await context.RetirementPayments.ToListAsync();
        }

        public async Task<IEnumerable<RetirementPayment>> GetAllRetirementPaymentsForUser(int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");
            return await context.RetirementPayments.Where(x => x.UserId == user.Id).ToListAsync();
        }

        public async Task<IEnumerable<RetirementPayment>> GetApprovedRetirementPayment(int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");

            var approvedRetirementPayment = await context.RetirementPayments.Where(x => x.IsApproved == true).ToListAsync();
            return approvedRetirementPayment;
        }

        public async Task<List<RetirementCashBookPayments>> GetCashRetirePayment(int retirementPaymentId)
        {
            return await context.RetirementCashBookPayments.Where(x => x.RetirePaymentId == retirementPaymentId).ToListAsync();
        }

        public async Task<RetirementPayment> GetRetirePayment(int retirementPaymentId, int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");
            return await context.RetirementPayments.FirstOrDefaultAsync(x => x.Id == retirementPaymentId);
        }

        public async Task<List<RetirementPaymentAction>> GetRetirePaymentAction(int retirementPaymentId)
        {
            return await context.RetirementPaymentActions.Where(x => x.RetirementPaymentId == retirementPaymentId).ToListAsync();
        }

        public async Task<List<RetirementPaymentFile>> GetRetirePaymentFiles(int retirementPaymentId)
        {
            return await context.RetirementPaymentFiles.Where(x => x.RetirementPaymentId == retirementPaymentId).ToListAsync();
        }

        public async Task<IEnumerable<RetirementPayment>> GetRetirePaymentsForRole(int RoleId, int userId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            if (user == null)
                throw new Exception("User does not exist");
            if (user.RoleLead == false)
            {
                var retirementPayments = await context.RetirementPayments.Where(x => x.IsApproved == false && x.UserId == user.Id && x.CurrentStage == 1).ToListAsync();
                return retirementPayments;
            }
            else if (user.RoleLead == true && (dept.Name != GeneralClass.Account && dept.Name != GeneralClass.Executive1 && dept.Name != GeneralClass.Executive2))
            {
                var retirementPayments = await context.RetirementPayments.Where(x => x.IsApproved == false && x.DeptId == user.DeptId && x.CurrentStage == 2).ToListAsync();
                return retirementPayments;
            }
            else if (dept.Name == GeneralClass.Account)
            {
                var retirementPayments = await context.RetirementPayments.Where(x => x.IsApproved == false && x.CurrentStage == 3).ToListAsync();
                return retirementPayments;
            }
            else if (dept.Name == GeneralClass.Executive1)
            {
                var retirementPayments = await context.RetirementPayments.Where(x => x.IsApproved == false && x.CurrentStage == 4).ToListAsync();
                return retirementPayments;
            }
            else if (dept.Name == GeneralClass.Executive2)
            {
                var retirementPayments = await context.RetirementPayments.Where(x => x.IsApproved == false && x.CurrentStage == 5).ToListAsync();
                return retirementPayments;
            }
            return null;
        }

        public async Task<IEnumerable<RetirementPayment>> GetUnApprovedRetirementPayment(int RoleId)
        {
            var role = await context.Roles.FindAsync(RoleId);
            if (role.Name != GeneralClass.Authorizer1 || role.Name != GeneralClass.Authorizer2 || role.Name != GeneralClass.ChiefAccountant)
                throw new Exception("Access Denied!");

            return await context.RetirementPayments.Where(x => x.IsApproved == false).ToListAsync();
        }
        public async Task<IEnumerable<CashAdvance>> GetUnRetiredCashAdvanceForUser(int UserId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User does not exist");
            return await context.CashAdvances.Where(x => x.UserId == UserId && x.Retired == false ).ToListAsync();
        }

        public async Task<RetirementPayment> PerformActionOnRetirePayment(int retirementPaymentId, int UserId, int RoleId, string Comment, ActionPerformed actionPerformed)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            var retirementPayment = await context.RetirementPayments.FindAsync(retirementPaymentId);
            if (retirementPayment.IsApproved == true)
                throw new Exception("Action failed, retirement payment is approved");
            if (retirementPayment == null)
                throw new Exception("Action failed, retirement payment does not exists");
            if (retirementPayment.CurrentStage == 6)
                throw new Exception("Action failed, retirement payment is approved.");
            if (retirementPayment.CurrentStage == 1 && (user.Id != retirementPayment.UserId))
                throw new Exception("Action failed, user can't access this.");
            if (retirementPayment.CurrentStage == 2 && (user.DeptId != retirementPayment.DeptId || user.RoleLead == false))
                throw new Exception("Action failed, user can't access this.");
            if (retirementPayment.CurrentStage == 3 && role.Name != GeneralClass.ChiefAccountant)
                throw new Exception("Action failed, user can't access this.");
            if (retirementPayment.CurrentStage == 4 && role.Name != GeneralClass.Authorizer1)
                throw new Exception("Action failed, user can't access this.");
            if (retirementPayment.CurrentStage == 5 && role.Name != GeneralClass.Authorizer2)
                throw new Exception("Action failed, user can't access this");

            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == retirementPayment.DeptId);
            retirementPayment.Dept = dept;

            context.Add(new RetirementPaymentAction
            {
                ActionPerformed = actionPerformed,
                Comment = Comment,
                UserId = UserId,
                RetirementPaymentId = retirementPayment.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });
            retirementPayment.DateUpdated = DateTime.Now;
            if (actionPerformed == ActionPerformed.Approved)
            {
                retirementPayment.CurrentStage += 1;
                if (retirementPayment.CurrentStage == 6)
                {
                    if (retirementPayment.CashAdvanceId != 0)
                    {
                        retirementPayment.IsApproved = true;
                        var cashAdvance = await context.CashAdvances.FirstOrDefaultAsync(x => x.Id == retirementPayment.CashAdvanceId);
                        cashAdvance.Retired = true;
                        context.Update(cashAdvance);
                    }
                    retirementPayment.IsApproved = true;
                }
            }
            else
            {
                var retirementPaymentUser = await context.Users.FindAsync(retirementPayment.UserId);
                if (retirementPaymentUser.RoleLead == false)
                {
                    retirementPayment.CurrentStage = 1;
                }
                else if (retirementPaymentUser.RoleLead == true && retirementPayment.Dept.Name == GeneralClass.Account)
                {
                    retirementPayment.CurrentStage = 3;
                }
                else if (retirementPaymentUser.RoleLead == true && retirementPayment.Dept.Name != GeneralClass.Account)
                {
                    retirementPayment.CurrentStage = 2;
                }
            }
            context.Update(retirementPayment);
            await context.SaveChangesAsync();
            return retirementPayment;
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
    }
}
