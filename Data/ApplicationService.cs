using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;
using VoucherAutomationSystem.Models.Enums;
using VoucherAutomationSystem.ViewModels;

namespace VoucherAutomationSystem.Data
{
    public class ApplicationService : IApplicationService
    {
        private readonly AppDbContext context;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IHostingEnvironment hostingEnvironment;

        public ApplicationService(RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager, AppDbContext context,
            IHostingEnvironment hostingEnvironment)
        {
            this.roleManager = roleManager;
            this.userManager = userManager;
            this.context = context;
            this.hostingEnvironment = hostingEnvironment;
        }

        public async Task<Voucher> AddNewVoucher(int UserId, int RoleId, VoucherViewModel voucherViewModel, List<CashBookViewModel> cashBookViewModels)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");
            if (voucherViewModel.ExchangeRate == null || voucherViewModel.ExchangeRate == 0)
                throw new Exception("Exchange rate cannot be equal to 0");

            if (role.Name != "AccountOfficer" && role.Name != "ChiefAccountant")
                throw new Exception("User cannot create a product.");
            //var voucherId = Guid.NewGuid();
            //create the product.
            var voucher = new Voucher
            {
                Name = voucherViewModel.Name,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                IsActive = false,
                CurrentLevelRoleName = role.Name,
                VoucherType = voucherViewModel.VoucherType,
                Description = voucherViewModel.Description,
                Payee = voucherViewModel.Payee,
                AccountNo = voucherViewModel.AccountNo,
                Currency = voucherViewModel.Currency,
                ChequeNo = voucherViewModel.ChequeNo,
                ExchangeRate = voucherViewModel.ExchangeRate,
                PhoneNo = voucherViewModel.PhoneNo,
                Address = voucherViewModel.Address,
                BankName = voucherViewModel.BankName,
                RoleCreator = role.Name
            };

            await context.AddAsync(voucher);
            await context.SaveChangesAsync();

            // Add the CashBook
            decimal totalAmount = 0;
            foreach (var cashBook in cashBookViewModels)
            {
                await context.AddAsync(new CashBook
                {
                    Amount = cashBook.Amount,
                    Description = cashBook.Description,
                    VoucherId = voucher.Id,
                    Particular = cashBook.Particular
                });
                totalAmount += cashBook.Amount;
            }
            totalAmount *= Convert.ToDecimal(voucherViewModel.ExchangeRate);

            //Add the Creation action.
            context.Add(new Models.Action
            {
                ActionPerformed = Models.Enums.ActionPerformed.Created,
                Comment = voucherViewModel.Comment,
                UserId = UserId,
                VoucherId = voucher.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId,
                UserName = role.Name
            });

            //update the CurrentLevelRole to the Next role if available
            if (totalAmount > 50000)
            {
                if (role.Name == "AccountOfficer")
                {
                    voucher.CurrentLevelRoleName = "ChiefAccountant";
                }
                else
                {
                    voucher.CurrentLevelRoleName = "Authorizer1";
                }
            }
            else
            {
                if (role.Name == "AccountOfficer")
                {
                    voucher.CurrentLevelRoleName = "ChiefAccountant";
                }
                else
                {
                    voucher.CurrentLevelRoleName = "Approval";
                    //fetch all users under Approval role.
                    var approvalUsers = await userManager.GetUsersInRoleAsync(voucher.CurrentLevelRoleName);
                    //fetch all Approval vouchers that havent been approved //isactive =  false.
                    var approvalVouchers = from vouch in context.ApprovalVouchers
                                where vouch.IsActive == false
                                select vouch;
                    //foreach (var approvalVouch in approvalVouchers)
                    //{
                    //    var vouch = await context.Vouchers.FirstOrDefaultAsync(x => x.Id == approvalVouch.VoucherId);
                    //    if (vouch.IsActive == true)
                    //    {
                    //        approvalVouch.IsActive = true;
                    //    }
                    //}
                    //int maxCount = int.MaxValue; int? chosenUserId = null;
                    //int maxCount = int.MaxValue;
                    //int? chosenUserId = null;
                    //foreach(var user in users){
                    //var count = approvalVouchers.Where(x => x,userId == userId);
                    //if(count < maxCount)
                    // chosenUserId = user.Id; naxCount = count;
                    //}
                    //foreach (var approvalUser in approvalUsers)
                    //{
                    //    if (approvalUser.IsActive == true)
                    //    {
                    //        var count = approvalVouchers.Count(x => x.UserId == approvalUser.Id);
                    //        if (count < maxCount)
                    //        {
                    //            chosenUserId = approvalUser.Id;
                    //            maxCount = count;
                    //        }
                    //    }

                    //}
                    var userGroups = approvalUsers.GroupBy(x => approvalVouchers.Count(y => y.UserId == x.Id));
                    var minCountUsers = userGroups.First(x => x.Key == userGroups.Min(y => y.Key)).ToList();
                    var idx = new Random().Next(minCountUsers.Count);
                    var chosenUserId = minCountUsers[idx];
                    //approvalVouchers.Add(userId  =chosenUserId)
                    await context.AddAsync(new ApprovalVoucher
                    {
                        UserId = chosenUserId.Id,
                        IsActive = voucher.IsActive,
                        VoucherId = voucher.Id
                    });
                }
            }
            voucher.TotalAmount = totalAmount;
            context.Update(voucher);

            await context.SaveChangesAsync();
            return voucher;
        }

        public async Task<Voucher> EditVoucher(Voucher voucher, List<CashBook> cashBooks, int UserId, int RoleId, string comment)
        {
            var newVoucher = await context.Vouchers.FindAsync(voucher.Id);
            voucher.DateCreated = newVoucher.DateCreated;
            context.Entry(newVoucher).State = EntityState.Detached;
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            if (voucher.IsActive == true)
                throw new Exception("Action failed, voucher is active");
            if (voucher.ExchangeRate == null || voucher.ExchangeRate == 0)
                throw new Exception("Exchange rate cannot be equal to 0");
            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            if (role.Name != "AccountOfficer" && role.Name != "ChiefAccountant")
                throw new Exception("This user does not have permission to edit a product.");

            context.Update(voucher);
            IEnumerable<CashBook> oldCashBook =  context.CashBooks.Where(x => x.VoucherId == voucher.Id);

            foreach (var cashBook in oldCashBook)
            {
                context.CashBooks.Remove(cashBook);
            }
            // Add the CashBook
            decimal totalAmount = 0;
            foreach (var cashBook in cashBooks)
            {
                await context.AddAsync(new CashBook
                {
                    Amount = cashBook.Amount,
                    Description = cashBook.Description,
                    VoucherId = voucher.Id,
                    Particular = cashBook.Particular
                });
                totalAmount += cashBook.Amount;
            }

            context.Add(new Models.Action
            {
                ActionPerformed = Models.Enums.ActionPerformed.Edited,
                UserId = UserId,
                VoucherId = voucher.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId,
                UserName = role.Name,
                Comment = comment
            });
            voucher.DateUpdated = DateTime.Now;
            totalAmount *= Convert.ToDecimal(voucher.ExchangeRate);
            voucher.TotalAmount = totalAmount;
            context.Update(voucher);
            //update the CurrentLevelRole to the Next role if available
            if (totalAmount > 50000)
            {
                if (role.Name == "AccountOfficer")
                {
                    voucher.CurrentLevelRoleName = "ChiefAccountant";
                }
                else
                {
                    voucher.CurrentLevelRoleName = "Authorizer1";
                }
            }
            else
            {
                if (role.Name == "AccountOfficer")
                {
                    voucher.CurrentLevelRoleName = "ChiefAccountant";
                }
                else
                {
                    voucher.CurrentLevelRoleName = "Approval";
                    var approvalVoucher = await context.ApprovalVouchers.FirstOrDefaultAsync(x => x.VoucherId == voucher.Id);
                    
                    if (approvalVoucher == null)
                    {
                        
                        var approvalUsers = await userManager.GetUsersInRoleAsync(voucher.CurrentLevelRoleName);

                        var approvalVouchers = from vouch in context.ApprovalVouchers
                                               where vouch.IsActive == false
                                               select vouch;


                        var userGroups = approvalUsers.GroupBy(x => approvalVouchers.Count(y => y.UserId == x.Id));
                        var minCountUsers = userGroups.First(x => x.Key == userGroups.Min(y => y.Key)).ToList();
                        var idx = new Random().Next(minCountUsers.Count);
                        var chosenUserId = minCountUsers[idx];
                        await context.AddAsync(new ApprovalVoucher
                        {
                            UserId = chosenUserId.Id,
                            IsActive = false,
                            VoucherId = voucher.Id
                        });
                    }
                    else
                    {
                        int approvalVoucherId = approvalVoucher.Id;
                        int? approvalUserId = approvalVoucher.UserId;
                        context.Entry(approvalVoucher).State = EntityState.Detached;

                        context.Update(new ApprovalVoucher
                        {
                            Id = approvalVoucherId,
                            IsActive = false,
                            VoucherId = voucher.Id,
                            UserId = approvalUserId
                        });
                    }
                     
                }
            }

            await context.SaveChangesAsync();
            return voucher;
        }
        public async Task<IEnumerable<Voucher>> GetAllVouchers(string RoleName)
        {
            if (RoleName == "Authorizer1" || RoleName == "Authorizer2" || RoleName == "ChiefAccountant" || RoleName == "AccountOfficer")
            {
                return await context.Vouchers.ToListAsync();
            }
            else if (RoleName == "Approval")
            {
                return await context.Vouchers.Where(x =>  x.TotalAmount <= 50000).ToListAsync();
            }
            else
            {
                return null;
            }
        }

        
        public async Task<IEnumerable<Voucher>> GetActiveVouchers(string RoleName)
        {
            if (RoleName == "Authorizer1" || RoleName == "Authorizer2" || RoleName == "ChiefAccountant" || RoleName == "AccountOfficer")
            {
                return await context.Vouchers.Where(x => x.IsActive == true ).ToListAsync();
            }
            else if ( RoleName == "Approval")
            {
                return await context.Vouchers.Where(x => x.IsActive == true && x.TotalAmount <= 50000).ToListAsync();
            }
            else
            {
                return null;
            }
            
        }
        public async Task<IEnumerable<Voucher>> GetInActiveVouchers(int RoleId)
        {
            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");
            if (role.Name != "ChiefAccountant" && role.Name != "AccountOfficer" && role.Name != "Authorizer1" && role.Name != "Authorizer2")
                throw new Exception("Invalid user role.");
            return await context.Vouchers.Where(x => x.IsActive == false).ToListAsync();

        }

        public async Task<Voucher> GetVoucher(int Id, string RoleName)
        {
            var voucher = await context.Vouchers.FindAsync(Id);

            if (RoleName == "Authorizer1" || RoleName == "Authorizer2" || RoleName == "ChiefAccountant")
            {
                voucher.TotalAmount /= Convert.ToDecimal(voucher.ExchangeRate);
                return voucher;
            }
            else if (RoleName == "Approval")
            {
                if (voucher.TotalAmount <= 50000)
                {
                    voucher.TotalAmount /= Convert.ToDecimal(voucher.ExchangeRate);
                    return voucher;
                }
                else
                {
                    return null;
                }
            }
            else if (RoleName == "AccountOfficer")
            {
                if (voucher.RoleCreator == "AccountOfficer" || voucher.TotalAmount <= 50000)
                {
                    voucher.TotalAmount /= Convert.ToDecimal(voucher.ExchangeRate);
                    return voucher;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
            
        }

        public async Task<List<CashBook>> GetCashbook(int Id)
        {
            return await context.CashBooks.Where(x => x.VoucherId == Id).ToListAsync();
        }

        public async Task<List<Models.Action>> GetVoucherActions(int voucherId)
        {
            var voucher = await context.Vouchers.FindAsync(voucherId);
            var actions = await context.Actions.Where(x => x.VoucherId == voucherId).ToListAsync();
            return actions;
        }

        public async Task<IEnumerable<Voucher>> GetVouchersForRole(int roleId, int userId)
        {
            var role = await context.Roles.FindAsync(roleId);
            if (role == null)
                throw new Exception("Invalid user role.");
            if (role.Name == "Approval")
            {
                List<Voucher> vouchers = new List<Voucher>();
                var approvalVouchers = await context.ApprovalVouchers.Where(x => x.UserId == userId && x.IsActive == false).ToListAsync();
                
                foreach (var approvalVoucher in approvalVouchers)
                {
                    var voucher = await context.Vouchers.FindAsync(approvalVoucher.VoucherId);
                    if (voucher.CurrentLevelRoleName == role.Name)
                    {
                        vouchers.Add(voucher);
                    }
                    
                    
                }
                return vouchers;
            }
            else
            {
                return await context.Vouchers.Where(x => x.CurrentLevelRoleName == role.Name).ToListAsync();
            } 
        }

        public async Task<Voucher> PerformActionOnVoucher(int VoucherId, int UserId, int RoleId, string Comment, ActionPerformed actionPerformed)
        {
            var voucher = await context.Vouchers.FindAsync(VoucherId);

            if (voucher.IsActive == true)
                throw new Exception("Action failed, voucher is active");

            if (voucher == null)
                throw new Exception("Invalid voucher.");

            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            if (role.Name != voucher.CurrentLevelRoleName)
                throw new Exception("Invalid user role.");


            if ((role.Name != "AccountOfficer" || role.Name != "ChiefAccountant") && (actionPerformed.Equals(ActionPerformed.Created) ||
                actionPerformed.Equals(ActionPerformed.Edited)))
                throw new Exception("User with this role cannot create or edit a product.");

            if ((role.Name == "AccountOfficer" /*|| role.Name != "Authorizer1" || role.Name != "Approval" || role.Name != "Authorizer2"*/) && ((actionPerformed.Equals(ActionPerformed.Approved) ||
               actionPerformed.Equals(ActionPerformed.Rejected))))
                throw new Exception("User with this role cannot approve or reject a product.");

            context.Actions.Add(new Models.Action
            {
                ActionPerformed = actionPerformed,
                Comment = Comment,
                UserId = UserId,
                VoucherId = voucher.Id,
                DateUpdated = DateTime.Now,
                RoleId = RoleId,
                UserName = role.Name
            });
            voucher.DateUpdated = DateTime.Now;
            if (voucher.TotalAmount > 50000)
            {
                // if the product was rejected, move it down one approval level
                if (actionPerformed.Equals(ActionPerformed.Rejected))
                {
                    voucher.CurrentLevelRoleName = voucher.RoleCreator;
                    await context.SaveChangesAsync();
                }

                else // if the product was created, edited or approved, move it up one approval level
                {
                    if (voucher.CurrentLevelRoleName == "ChiefAccountant")
                    {
                        voucher.CurrentLevelRoleName = "Authorizer1";
                    }
                    else if (voucher.CurrentLevelRoleName == "Authorizer1")
                    {
                        voucher.CurrentLevelRoleName = "Authorizer2";
                    }
                    else
                    {
                        voucher.CurrentLevelRoleName = null;
                    }
                    // if the current approval level has no other layer above it, activate the product
                    if (voucher.CurrentLevelRoleName == null) voucher.IsActive = true;
                    await context.SaveChangesAsync();
                }
            }
            else
            {
                // if the product was rejected, move it down one approval level
                if (actionPerformed.Equals(ActionPerformed.Rejected))
                {
                    if (role.Name == "Approval")
                    {
                        voucher.CurrentLevelRoleName = voucher.RoleCreator;
                        var approvalUsers = await userManager.GetUsersInRoleAsync(voucher.CurrentLevelRoleName);
                        var approvalVoucher = await context.ApprovalVouchers.FirstOrDefaultAsync(x => x.VoucherId == voucher.Id);
                        int newId = approvalVoucher.Id;
                        context.Entry(approvalVoucher).State = EntityState.Detached;
                        context.Update(new ApprovalVoucher
                        {
                            Id = approvalVoucher.Id,
                            IsActive = true,
                            VoucherId = voucher.Id,
                            UserId = UserId
                        });
                    }
                    else
                    {
                        voucher.CurrentLevelRoleName = voucher.RoleCreator;
                    }
                    await context.SaveChangesAsync();
                }

                else // if the product was created, edited or approved, move it up one approval level
                {
                    if (voucher.CurrentLevelRoleName == "ChiefAccountant")
                    {
                        voucher.CurrentLevelRoleName = "Approval";
                        var approvalUsers = await userManager.GetUsersInRoleAsync(voucher.CurrentLevelRoleName);

                        var approvalVouchers = from vouch in context.ApprovalVouchers
                                               where vouch.IsActive == false
                                               select vouch;
                        
                        var approvalvoucher = await context.ApprovalVouchers.FirstOrDefaultAsync(x => x.VoucherId == voucher.Id);

                        var userGroups = approvalUsers.GroupBy(x => approvalVouchers.Count(y => y.UserId == x.Id));
                        var minCountUsers = userGroups.First(x => x.Key == userGroups.Min(y => y.Key)).ToList();
                        var idx = new Random().Next(minCountUsers.Count);
                        var chosenUserId = minCountUsers[idx];

                        if (approvalvoucher == null)
                        {
                            await context.AddAsync(new ApprovalVoucher
                            {
                                UserId = chosenUserId.Id,
                                IsActive = false,
                                VoucherId = voucher.Id
                            });
                        }
                        else
                        {
                            int approvalVoucherId = approvalvoucher.Id;
                            int? approvalVoucherUserId = approvalvoucher.UserId;

                            context.Entry(approvalvoucher).State = EntityState.Detached;
                            context.Update(new ApprovalVoucher
                            {
                                Id = approvalVoucherId,
                                IsActive = false,
                                UserId = approvalVoucherUserId,
                                VoucherId = voucher.Id
                            });
                        }
                        
                    }
                    else
                    {
                        voucher.CurrentLevelRoleName = null;
                        var approvalVoucher = await context.ApprovalVouchers.FirstOrDefaultAsync(x => x.VoucherId == voucher.Id);
                        approvalVoucher.IsActive = true;
                    }
                    // if the current approval level has no other layer above it, activate the product
                    if (voucher.CurrentLevelRoleName == null) voucher.IsActive = true;
                    await context.SaveChangesAsync();
                }

            }
            return voucher;

        }

        public async Task<List<VoucherFile>> GetVoucherFiles(int voucherId)
        {
            return await context.VoucherFiles.Where(x => x.VoucherId == voucherId).ToListAsync();
        }

        public async Task<Voucher> EditVoucherFiles(int voucherId, VoucherFileViewModel uploadFiles, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var voucher = await context.Vouchers.FindAsync(voucherId);
            if (voucher == null)
                throw new Exception("voucher not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            if (role.Name != "AccountOfficer" && role.Name != "ChiefAccountant")
                throw new Exception("User cannot create a product.");

            for (int i = 0; i < uploadFiles.Name.Count; i++)
            {
                var fileUrl = SavePhoto(uploadFiles.File[i]);
                context.Update(new VoucherFile
                {
                    Name = uploadFiles.Name[i],
                    FileUrl = fileUrl,
                    VoucherId = voucherId
                });
            }
            await context.SaveChangesAsync();
            return voucher;
        }

        public async Task<Voucher> AddVoucherFiles(int voucherId, List<IFormFile> files, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var voucher = await context.Vouchers.FindAsync(voucherId);
            if (voucher == null)
                throw new Exception("voucher not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            if (role.Name != "AccountOfficer" && role.Name != "ChiefAccountant")
                throw new Exception("User cannot create a voucher.");
            if (voucher.CurrentLevelRoleName != "ChiefAccountant" && voucher.CurrentLevelRoleName != "AccountOfficer" && voucher.CurrentLevelRoleName != "Authorizer1" && voucher.CurrentLevelRoleName != "Approval")
                throw new Exception("Voucher file cannot be created at this point.");
            foreach (var file in files)
            {
                var fileUrl = SavePhoto(file);
                await context.AddAsync(new VoucherFile
                {
                    FileUrl = fileUrl,
                    VoucherId = voucherId
                });
            }
            await context.SaveChangesAsync();
            return voucher;
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

        public async Task<VoucherFile> DeleteVoucherFiles(int voucherId, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var voucherFile = await context.VoucherFiles.FindAsync(voucherId);
            var voucher = await context.Vouchers.FindAsync(voucherFile.VoucherId);
            if (voucher.CurrentLevelRoleName != "AccountOfficer" && voucher.CurrentLevelRoleName != "ChiefAccountant" && voucher.CurrentLevelRoleName != "Approval" && voucher.CurrentLevelRoleName != "Authorizer1")
                throw new Exception("voucher file not found.");
            if (voucherFile == null)
                throw new Exception("voucher file not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            if (role.Name != "AccountOfficer" && role.Name != "ChiefAccountant")
                throw new Exception("User cannot delete a file.");
            context.VoucherFiles.Remove(voucherFile);
            await context.SaveChangesAsync();
            return voucherFile;
        }


        public async Task<DashBoardViewModel> DashBoard()
        {
            var dashBoardViewModel = new DashBoardViewModel
            {
                ActiveCount = await context.Vouchers.Where(x => x.IsActive == true).CountAsync(),
                InactiveCount = await context.Vouchers.Where(x => x.IsActive == false).CountAsync(),
                ApprovalCount = await context.Actions.Where(x => x.ActionPerformed == ActionPerformed.Approved).CountAsync(),
                RejectedCount = await context.Actions.Where(x => x.ActionPerformed == ActionPerformed.Rejected).CountAsync(),
                EditedCount = await context.Actions.Where(x => x.ActionPerformed == ActionPerformed.Edited).CountAsync(),
                CreatedCount = await context.Actions.Where(x => x.ActionPerformed == ActionPerformed.Created).CountAsync(),
                JanCount = await context.Vouchers.Where(x => x.DateCreated.Month == 1).CountAsync(),
                FebCount = await context.Vouchers.Where(x => x.DateCreated.Month == 2).CountAsync(),
                MarchCount = await context.Vouchers.Where(x => x.DateCreated.Month == 3).CountAsync(),
                AprilCount = await context.Vouchers.Where(x => x.DateCreated.Month == 4).CountAsync(),
                MayCount = await context.Vouchers.Where(x => x.DateCreated.Month == 5).CountAsync(),
                JuneCount = await context.Vouchers.Where(x => x.DateCreated.Month == 6).CountAsync(),
                JulyCount = await context.Vouchers.Where(x => x.DateCreated.Month == 7).CountAsync(),
                AugCount = await context.Vouchers.Where(x => x.DateCreated.Month == 8).CountAsync(),
                SeptCount = await context.Vouchers.Where(x => x.DateCreated.Month == 9).CountAsync(),
                OctCount = await context.Vouchers.Where(x => x.DateCreated.Month == 10).CountAsync(),
                NovCount = await context.Vouchers.Where(x => x.DateCreated.Month == 11).CountAsync(),
                DecCount = await context.Vouchers.Where(x => x.DateCreated.Month == 12).CountAsync()
            };

            return dashBoardViewModel;
        }

        public async Task<CashBook> DeleteCashBook(int voucherId, int UserId, int RoleId)
        {
            var user = await context.Users.FindAsync(UserId);
            if (user == null)
                throw new Exception("User not found.");

            var cashBook = await context.CashBooks.FindAsync(voucherId);
            var voucher = await context.Vouchers.FindAsync(cashBook.VoucherId);
            if (voucher.CurrentLevelRoleName != "AccountOfficer" && voucher.CurrentLevelRoleName != "ChiefAccountant" && voucher.CurrentLevelRoleName != "Approval" && voucher.CurrentLevelRoleName != "Authorizer1")
                throw new Exception("Cashbook cannot be deleted.");
            if (cashBook== null)
                throw new Exception("Cashbook not found.");

            var role = await context.Roles.FindAsync(RoleId);
            if (role == null)
                throw new Exception("Invalid user role.");

            if (role.Name != "AccountOfficer" && role.Name != "ChiefAccountant")
                throw new Exception("User cannot delete a file.");
            context.CashBooks.Remove(cashBook);
            await context.SaveChangesAsync();
            return cashBook;
        }
    }
}
