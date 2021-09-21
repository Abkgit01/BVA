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
    public class PettyCashService : IPettyCashService
    {
        private readonly AppDbContext context;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHostingEnvironment hostingEnvironment;

        public PettyCashService(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, AppDbContext context,
            IHostingEnvironment hostingEnvironment)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.context = context;
            this.hostingEnvironment = hostingEnvironment;
        }

        public async Task<PettyCash> AddNewPettyCash(int UserId, int RoleId, PettyCashViewModel pettyCashViewModel)
        {
            if (pettyCashViewModel.TotalAmount > 5000)
                throw new Exception("Amount cannot be greater than N5000");
            //Get user 
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found, login in add petty cash");
            //Get role
            var role = await context.Roles.FindAsync(RoleId);
            if(role == null)
                throw new Exception("Role not found, user cant create petty cash");
            //Get department
            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            if (dept == null)
                throw new Exception("User not in any department.");
            var pettycash = new PettyCash
            {
                UserId = user.Id,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                DeptId = dept.ID,
                Dept = dept,
                IsApproved = false,
                Description = pettyCashViewModel.Description,
                User = user,
                RoleId = role.Id,
                Role = role,
                TotalAmount = pettyCashViewModel.TotalAmount,
                CurrentStage = 2
            };
            await context.AddAsync(pettycash);
            await context.SaveChangesAsync();

            

            //Add new Petty Cash action
            context.Add(new PettyCashAction
            {
                ActionPerformed = Models.Enums.ActionPerformed.Created,
                Comment = pettyCashViewModel.Comment,
                UserId = UserId,
                PettyCashId = pettycash.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });

            

            //get user to approve
            var usersToApprove = await context.Users.Where(x => x.RoleLead == true).ToListAsync();
            foreach (var userToApprove in usersToApprove.ToList())
            {
                var roles = await userManager.GetRolesAsync(userToApprove);
                var roleName = await roleManager.FindByNameAsync(roles.SingleOrDefault());
                if ( roleName.Name == GeneralClass.Authorizer1 || roleName.Name == GeneralClass.Authorizer2 || userToApprove.Id == pettycash.UserId)
                {
                    usersToApprove.Remove(userToApprove);
                }
            }
            int count = usersToApprove.Count();
            var selectedNumber = new Random().Next(count);
            var selectedUser = usersToApprove[selectedNumber];

            await context.AddAsync(new PettyCashApproval
            {
                UserId = selectedUser.Id,
                IsActive = false,
                PettyCashID = pettycash.Id
            });
            pettycash.UserToApprove = selectedUser.Id;
            context.Update(pettycash);

            await context.SaveChangesAsync();

            return pettycash;
        }

        public async Task<PettyCash> AddPettyCashFiles(int PettyCashId, List<IFormFile> files, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var pettyCash = await context.PettyCashes.FindAsync(PettyCashId);
            if (pettyCash == null)
                throw new Exception("Petty cash not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            foreach (var file in files)
            {
                var fileUrl = SavePhoto(file);
                await context.AddAsync(new PettyCashFile
                {
                    Name = fileUrl,
                    PettyCashId = PettyCashId
                });
            }
            await context.SaveChangesAsync();
            return pettyCash;
        }

        //public async Task<PettyCashBook> DeletePettyCashBook(int UserId, int RoleId, int PettyCashId)
        //{
        //    var user = await context.Users.FindAsync(UserId);
        //    if (user == null)
        //        throw new Exception("User not found.");

        //    var pettyCashBook = await context.pettyCashBooks.FindAsync(PettyCashId);
        //    var pettyCash = await context.PettyCashes.FindAsync(pettyCashBook.PettyCashId);

        //    if (pettyCash.IsApproved == true)
        //        throw new Exception("Petty Cash is approved, cant be deleted!");
        //    if (pettyCash.UserId != UserId)
        //        throw new Exception("Payment row cannot be deleted by this user.");
        //    if (pettyCashBook == null)
        //        throw new Exception("Payment row not found.");

        //    var role = await context.Roles.FindAsync(RoleId);
        //    if (role == null)
        //        throw new Exception("Invalid user role.");

        //    context.pettyCashBooks.Remove(pettyCashBook);
        //    await context.SaveChangesAsync();
        //    return pettyCashBook;
        //}

        public async Task<PettyCashFile> DeletePettyCashFile(int PettyCashFileId, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var pettyCashFile = await context.pettyCashFiles.FindAsync(PettyCashFileId);
            var pettyCash = await context.CashAdvances.FindAsync(pettyCashFile.PettyCashId);
            if (pettyCash.UserId != UserId)
                throw new Exception("Petty cash file cannot be deleted by this user.");
            if (pettyCash.IsApproved == true)
                throw new Exception("Petty Cash is approved, cant be deleted!");
            if (pettyCashFile == null)
                throw new Exception("Petty cash File not found.");

            context.pettyCashFiles.Remove(pettyCashFile);
            await context.SaveChangesAsync();
            return pettyCashFile;
        }

        public async Task<PettyCash> EditPettyCash(PettyCash pettyCash, int UserId, int RoleId, string comment)
        {
            if (pettyCash.TotalAmount > 5000)
                throw new Exception("Amount cannot be greater than N5000");
            var newPettyCash = await context.PettyCashes.FindAsync(pettyCash.Id);
            newPettyCash.DateUpdated = DateTime.Now;
            newPettyCash.Description = pettyCash.Description;
            newPettyCash.TotalAmount = pettyCash.TotalAmount;

            context.Entry(pettyCash).State = EntityState.Detached;
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            if (newPettyCash.IsApproved == true)
                throw new Exception("Action failed, Petty Cash is approved");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            if (UserId != newPettyCash.UserId)
                throw new Exception("This user does not have permission to edit this petty cash.");

            var dept = await context.Departments.FirstOrDefaultAsync(x => x.ID == user.DeptId);
            if (dept == null)
                throw new Exception("This user does not have permission to edit this Petty Cash.");
            context.PettyCashes.Update(newPettyCash);

            

            

            //Add new petty cash action
            context.Add(new PettyCashAction
            {
                ActionPerformed = Models.Enums.ActionPerformed.Edited,
                Comment = comment,
                UserId = UserId,
                PettyCashId = newPettyCash.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });
            newPettyCash.CurrentStage = 2;


            //set current stage
            var pettyCashApproval = await context.PettyCashApprovals.FirstOrDefaultAsync(x => x.PettyCashID == newPettyCash.Id);
            pettyCashApproval.IsActive = false;
            context.PettyCashApprovals.Update(pettyCashApproval);
            await context.SaveChangesAsync();

            return newPettyCash;
        }

        public async Task<IEnumerable<PettyCash>> GetAllPettyCash(int UserId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User does not exist");
            return await context.PettyCashes.ToListAsync();
        }
        public async Task<IEnumerable<PettyCash>> GetAllPettyCashForUser(int UserId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User does not exist");
            return await context.PettyCashes.Where(x => x.UserId == user.Id).ToListAsync();
        }


        public async Task<IEnumerable<PettyCash>> GetApprovedPettyCash(int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");

            var activePettyCash = await context.PettyCashes.Where(x => x.IsApproved == true).ToListAsync();
            foreach (var petty in activePettyCash)
            {
                petty.User = await context.Users.FindAsync(petty.UserId);
            }
            return activePettyCash;
        }

        public async Task<IEnumerable<PettyCash>> GetPendingPettyCashForUser(int RoleId, int userId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("User does not exist");
            List<PettyCash> pendingPettyCash;
            pendingPettyCash = await context.PettyCashes.Where(x => x.IsApproved == false && x.CurrentStage == 1 && x.UserId == user.Id).ToListAsync();
            if (user.RoleLead == true)
            {
                var pettyCashToApprove = await context.PettyCashApprovals.Where(x  => x.UserId == user.Id && x.IsActive == false).ToListAsync();
                foreach (var petty in pettyCashToApprove)
                {
                    var pettyCash = await context.PettyCashes.FirstOrDefaultAsync(x => x.Id == petty.PettyCashID);
                    pendingPettyCash.Add(pettyCash);
                }
            }
            foreach (var pettyCash1 in pendingPettyCash)
            {
                pettyCash1.User = await context.Users.FirstOrDefaultAsync(x => x.Id == pettyCash1.UserId);
            }
            return pendingPettyCash;
        }

        public async Task<PettyCash> GetPettyCash(int pettyCashId, int UserId)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == UserId);
            if (user == null)
                throw new Exception("User does not exist");
            var pettyCash = await context.PettyCashes.FirstOrDefaultAsync(x => x.Id == pettyCashId);
             pettyCash.User = await context.Users.FirstOrDefaultAsync(x => x.Id == pettyCash.UserId);
            return pettyCash;
        }

        public async Task<List<PettyCashAction>> GetPettyCashActions(int pettyCashId)
        {
            return await context.pettyCashActions.Where(x => x.PettyCashId == pettyCashId).ToListAsync();
        }

        //public async Task<List<PettyCashBook>> GetPettyCashBook(int pettyCashId)
        //{
        //    return await context.pettyCashBooks.Where(x => x.PettyCashId == pettyCashId).ToListAsync();
        //}

        public async Task<List<PettyCashFile>> GetPettyCashFiles(int PettyCashId)
        {
            return await context.pettyCashFiles.Where(x => x.PettyCashId == PettyCashId).ToListAsync();
        }

        public async Task<IEnumerable<PettyCash>> GetUnApprovedPettyCash(int UserId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("user does not exist");

            var inActivePettyCash = await context.PettyCashes.Where(x => x.IsApproved == false).ToListAsync();

            foreach (var petty in inActivePettyCash)
            {
                petty.User = await context.Users.FindAsync(petty.UserId);
            }
            return inActivePettyCash;
        }

        public async Task<PettyCash> PerformActionOnPettyCash(int PettyCashId, int UserId, int RoleId, string Comment, ActionPerformed actionPerformed)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");
            if (user.RoleLead == false)
                throw new Exception("User cannot perform action on this petty cash.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            var pettyCash = await context.PettyCashes.FindAsync(PettyCashId);
            if (pettyCash.IsApproved == true)
                throw new Exception("Action failed, Petty Cash is approved");
            if (pettyCash == null)
                throw new Exception("Action failed, Petty Cash does not exists");
            if (pettyCash.CurrentStage == 1)
                throw new Exception("Action failed, Petty Cash cannot be approved or rejected yet");

            context.Add(new PettyCashAction
            {
                ActionPerformed = actionPerformed,
                Comment = Comment,
                UserId = UserId,
                PettyCashId = pettyCash.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId
            });
            pettyCash.DateUpdated = DateTime.Now;
            if (actionPerformed == ActionPerformed.Approved)
            {
                var pettyCashApproval = await context.PettyCashApprovals.FirstOrDefaultAsync(x => x.PettyCashID == pettyCash.Id);
                pettyCashApproval.IsActive = true;
                context.PettyCashApprovals.Update(pettyCashApproval);
                pettyCash.IsApproved = true;
                pettyCash.CurrentStage = 3;
            }
            else
            {
                var pettyCashApproval = await context.PettyCashApprovals.FirstOrDefaultAsync(x => x.PettyCashID == pettyCash.Id);
                pettyCashApproval.IsActive = true;
                context.PettyCashApprovals.Update(pettyCashApproval);
                pettyCash.CurrentStage = 1;
            }
            await context.SaveChangesAsync();
            return pettyCash;
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
