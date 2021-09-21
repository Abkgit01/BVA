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

    public class AdvancePaymentService : IAdvancePaymentService
    {
        private readonly AppDbContext context;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHostingEnvironment hostingEnvironment;

        public AdvancePaymentService(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, AppDbContext context,
            IHostingEnvironment hostingEnvironment)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.context = context;
            this.hostingEnvironment = hostingEnvironment;
        }
        

        public async Task<CashAdvance> AddNewCashAdvance(int UserId, int RoleId, CashAdvanceViewModel cashAdvanceViewModel, List<CashAdvancePaymentsViewModel> cashAdvancePaymentsViewModels)
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
            var cashAdvance = new CashAdvance
            {
                Name = cashAdvanceViewModel.FirstName + " " + cashAdvanceViewModel.LastName,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                IsApproved = false,
                CurrentStage = 2,
                ExchangeRate = cashAdvanceViewModel.ExchangeRate,
                DeptId = cashAdvanceViewModel.DeptId,
                Description = cashAdvanceViewModel.Description,
                Currency = cashAdvanceViewModel.Currency,
                TotalAmount = cashAdvanceViewModel.TotalAmount,
                RoleId = role.Id,
                Role = role,
                UserId = user.Id,
                User = user,
                Dept = dept,
                Retired = false
            };
            await context.AddAsync(cashAdvance);

            await context.SaveChangesAsync();
            //Add list of Cash advance payment and get total amount
            decimal totalAmount = 0;
            foreach (var cashAdvancePayments in cashAdvancePaymentsViewModels)
            {
                await context.AddAsync(new CashAdvancePayment
                {
                    Amount = cashAdvancePayments.Amount,
                    Description = cashAdvancePayments.Description,
                    CashAdvanceId = cashAdvance.Id
                });
                totalAmount += cashAdvancePayments.Amount;
            }
            //Add new advance payment action
            context.Add(new CashAdvanceAction
            {
                ActionPerformed = Models.Enums.ActionPerformed.Created,
                Comment = cashAdvanceViewModel.Comment,
                UserId = UserId,
                CashAdvanceId = cashAdvance.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });
            //Total amount
            cashAdvance.TotalAmount = totalAmount;
            //set current stage 
            if (dept.Name == GeneralClass.Account && user.RoleLead == false)
            {
                cashAdvance.CurrentStage = 3;
            }
            if (user.RoleLead == true && role.Name != GeneralClass.ChiefAccountant)
            {
                cashAdvance.CurrentStage = 3;
            }
            else if (user.RoleLead == true && role.Name == GeneralClass.ChiefAccountant)
            {
                cashAdvance.CurrentStage = 4;
            }
            context.Update(cashAdvance);
            await context.SaveChangesAsync();

            return cashAdvance;
        }
        public async Task<CashAdvance> AddCashAdvanceFiles(int cashAdvanceId, List<IFormFile> files, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var cashAdvance = await context.CashAdvances.FindAsync(cashAdvanceId);
            if (cashAdvance == null)
                throw new Exception("Cash Advance not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            foreach (var file in files)
            {
                var fileUrl = SavePhoto(file);
                await context.AddAsync(new CashAdvanceFile
                {
                    Name = fileUrl,
                    CashAdvanceId = cashAdvanceId
                });
            }
            await context.SaveChangesAsync();
            return cashAdvance;
        }
        public async Task<CashAdvancePayment> DeleteAdvancePayments(int UserId, int RoleId, int CashAdvancePaymentId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var cashAdvancePayment = await context.CashAdvancePayments.FindAsync(CashAdvancePaymentId);
            var cashAdvance = await context.CashAdvances.FindAsync(cashAdvancePayment.CashAdvanceId);
            if (cashAdvance.UserId != UserId)
                throw new Exception("Cash Advance Payment cannot be deleted by this user.");
            if (cashAdvancePayment == null)
                throw new Exception("Cash Advance Payment not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            context.CashAdvancePayments.Remove(cashAdvancePayment);
            await context.SaveChangesAsync();
            return cashAdvancePayment;
        }

        public async Task<CashAdvanceFile> DeleteCashAdvanceFile(int CashAdvanceFileId, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var cashAdvanceFile = await context.CashAdvanceFiles.FindAsync(CashAdvanceFileId);
            var cashAdvance = await context.CashAdvances.FindAsync(cashAdvanceFile.CashAdvanceId);
            if (cashAdvance.UserId != UserId)
                throw new Exception("Cash Advance file cannot be deleted by this user.");
            if (cashAdvance.IsApproved == true)
                throw new Exception("Cash Advance is approved, cant be deleted!");
            if (cashAdvanceFile == null)
                throw new Exception("Cash Advance File not found.");

            context.CashAdvanceFiles.Remove(cashAdvanceFile);
            await context.SaveChangesAsync();
            return cashAdvanceFile;
        }

        public async Task<CashAdvance> EditCashAdvance(CashAdvance cashAdvance, List<CashAdvancePayment> cashAdvancePayments, int UserId, int RoleId, string comment)
        {
            var newCashAdvance = await context.CashAdvances.FindAsync(cashAdvance.Id);
            newCashAdvance.ExchangeRate = cashAdvance.ExchangeRate;
            newCashAdvance.Description = cashAdvance.Description;
            newCashAdvance.Currency = cashAdvance.Currency;

            context.Entry(cashAdvance).State = EntityState.Detached;
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            if (newCashAdvance.IsApproved == true)
                throw new Exception("Action failed, Cash advance is approved");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            if (UserId != newCashAdvance.UserId)
                throw new Exception("This user does not have permission to edit a product.");

            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            if(dept == null)
                throw new Exception("This user does not have permission to edit a product.");

            newCashAdvance.CurrentStage = 2;
            context.Update(newCashAdvance);

            IEnumerable<CashAdvancePayment> oldCashPayement = context.CashAdvancePayments.Where(x => x.CashAdvanceId == newCashAdvance.Id);

            foreach (var cashPayment in oldCashPayement)
            {
                context.CashAdvancePayments.Remove(cashPayment);
            }
            // Add the CashBook
            decimal totalAmount = 0;
            foreach (var cashPayment in cashAdvancePayments)
            {
                await context.AddAsync(new CashAdvancePayment
                {
                    Amount = cashPayment.Amount,
                    Description = cashPayment.Description,
                    CashAdvanceId = newCashAdvance.Id
                });
                totalAmount += cashPayment.Amount;
            }

            //Add new advance payment action
            context.Add(new CashAdvanceAction
            {
                ActionPerformed = Models.Enums.ActionPerformed.Edited,
                Comment = comment,
                UserId = UserId,
                CashAdvanceId = newCashAdvance.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });
            newCashAdvance.DateUpdated = DateTime.Now;
            newCashAdvance.TotalAmount = totalAmount;
            //set current stage 
            if (dept.Name == GeneralClass.Account && user.RoleLead == false)
            {
                newCashAdvance.CurrentStage = 3;
            }
            if (user.RoleLead == true && role.Name != GeneralClass.ChiefAccountant)
            {
                newCashAdvance.CurrentStage = 3;
            }
            else if (user.RoleLead == true && role.Name == GeneralClass.ChiefAccountant)
            {
                newCashAdvance.CurrentStage = 4;
            }
            context.Update(newCashAdvance);
            await context.SaveChangesAsync();

            return newCashAdvance;
        }

        public async Task<IEnumerable<CashAdvance>> GetActiveCashAdvance(int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");

            var activeCashAdvance = await context.CashAdvances.Where(x => x.IsApproved == true).ToListAsync();
            return activeCashAdvance;
        }

        public async Task<IEnumerable<CashAdvance>> GetAllCashAdvance(int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");
            return await context.CashAdvances.ToListAsync();
        }
        public async Task<IEnumerable<CashAdvance>> GetAllCashAdvanceCreatedByUser(int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");
            return await context.CashAdvances.Where(x => x.UserId == user.Id).ToListAsync();
        }

        public async Task<CashAdvance> GetCashAdvance(int cashAdvanceId, int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");
            return await context.CashAdvances.FirstOrDefaultAsync(x => x.Id == cashAdvanceId);
        }

        public async Task<List<CashAdvanceAction>> GetCashAdvanceAction(int cashAdvanceId)
        {
            return await context.CashAdvanceActions.Where(x => x.CashAdvanceId == cashAdvanceId).ToListAsync();
        }

        public async Task<List<CashAdvanceFile>> GetCashAdvanceFiles(int cashAdvanceId)
        {
            return await context.CashAdvanceFiles.Where(x => x.CashAdvanceId == cashAdvanceId).ToListAsync();
        }

        public async Task<IEnumerable<CashAdvance>> GetCashAdvanceForRole(int RoleId, int userId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            if (user == null)
                throw new Exception("User does not exist");
            if (user.RoleLead == false)
            {
                var cashAdvances = await context.CashAdvances.Where(x => x.IsApproved == false  && x.UserId == user.Id && x.CurrentStage == 1).ToListAsync();
                return cashAdvances;
            }
            else if (user.RoleLead == true && (dept.Name != GeneralClass.Account && dept.Name != GeneralClass.Executive1 && dept.Name != GeneralClass.Executive2))
            {
                var cashAdvances = await context.CashAdvances.Where(x => x.IsApproved == false && x.DeptId == user.DeptId && x.CurrentStage == 2).ToListAsync();
                return cashAdvances;
            }
            else if (dept.Name == GeneralClass.Account)
            {
                var cashAdvances = await context.CashAdvances.Where(x => x.IsApproved == false  && x.CurrentStage == 3).ToListAsync();
                return cashAdvances;
            }
            else if(dept.Name == GeneralClass.Executive1)
            {
                var cashAdvances = await context.CashAdvances.Where(x => x.IsApproved == false && x.CurrentStage == 4).ToListAsync();
                return cashAdvances;
            }
            else if (dept.Name == GeneralClass.Executive2)
            {
                var cashAdvances = await context.CashAdvances.Where(x => x.IsApproved == false && x.CurrentStage == 5).ToListAsync();
                return cashAdvances;
            }
            return null;
        }

        public async Task<List<CashAdvancePayment>> GetCashAdvancePayments(int cashAdvanceId)
        {
            return await context.CashAdvancePayments.Where(x => x.CashAdvanceId == cashAdvanceId).ToListAsync();
        }

        public async Task<IEnumerable<CashAdvance>> GetInActiveCashAdvance(int RoleId)
        {
            var role = await context.Roles.FindAsync(RoleId);
            if (role.Name != GeneralClass.Authorizer1 || role.Name != GeneralClass.Authorizer2 || role.Name != GeneralClass.ChiefAccountant)
                throw new Exception("Access Denied!");

            return await context.CashAdvances.Where(x => x.IsApproved == false).ToListAsync();
        }

        public async Task<CashAdvance> PerformActionOnCashAdvance(int CashAdvanceId, int UserId, int RoleId, string Comment, ActionPerformed actionPerformed)
        {
            
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            var cashAdvance = await context.CashAdvances.FindAsync(CashAdvanceId);
            if (cashAdvance.IsApproved == true)
                throw new Exception("Action failed, Cash advance is approved");
            if (cashAdvance == null)
                throw new Exception("Action failed, Cash advance does not exists");
            if (cashAdvance.CurrentStage == 6)
                throw new Exception("Action failed, Cash advance is approved.");
            if (cashAdvance.CurrentStage == 1 && (user.Id != cashAdvance.UserId) )
                throw new Exception("Action failed, user can't access this.");
            if (cashAdvance.CurrentStage == 2 && (user.DeptId != cashAdvance.DeptId || user.RoleLead == false))
                throw new Exception("Action failed, user can't access this.");
            if (cashAdvance.CurrentStage == 3 && role.Name != GeneralClass.ChiefAccountant)
                throw new Exception("Action failed, user can't access this.");
            if (cashAdvance.CurrentStage == 4 && role.Name != GeneralClass.Authorizer1)
                throw new Exception("Action failed, user can't access this.");
            if (cashAdvance.CurrentStage == 5 && role.Name != GeneralClass.Authorizer2)
                throw new Exception("Action failed, user can't access this");

            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == cashAdvance.DeptId);
            cashAdvance.Dept = dept;

            context.Add(new CashAdvanceAction
            {
                ActionPerformed = actionPerformed,
                Comment = Comment,
                UserId = UserId,
                CashAdvanceId = cashAdvance.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });
            cashAdvance.DateUpdated = DateTime.Now;
            if (actionPerformed == ActionPerformed.Approved)
            {
                cashAdvance.CurrentStage += 1;
                if (cashAdvance.CurrentStage == 6)
                {
                    cashAdvance.IsApproved = true;
                }
            }
            else
            {
                var cashAdvanceUser = await context.Users.FindAsync(cashAdvance.UserId);
                if (cashAdvanceUser.RoleLead == false)
                {
                    cashAdvance.CurrentStage = 1;
                }
                else if (cashAdvanceUser.RoleLead == true && cashAdvance.Dept.Name == GeneralClass.Account)
                {
                    cashAdvance.CurrentStage = 3;
                }
                else if (cashAdvanceUser.RoleLead == true && cashAdvance.Dept.Name != GeneralClass.Account)
                {
                    cashAdvance.CurrentStage = 2;
                }
            }
            context.Update(cashAdvance);
            await context.SaveChangesAsync();
            return cashAdvance;
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
