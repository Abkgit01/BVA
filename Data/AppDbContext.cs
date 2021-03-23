using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {

        }
        public DbSet<Particular> Particulars { get; set; }
        public DbSet<Models.Action> Actions { get; set; }
        public DbSet<CashBook> CashBooks { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherFile> VoucherFiles { get; set; }
        public DbSet<ApprovalVoucher> ApprovalVouchers { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationRole>().HasIndex(x => x.Name).IsUnique();
            builder.Entity<Voucher>()
            .HasOne(s => s.Role)
            .WithMany(c => c.vouchers)
            .HasForeignKey(s => s.CurrentLevelRoleName)
            .HasPrincipalKey(c => c.Name);
            builder.Entity<ApplicationRole>().HasData(
                new ApplicationRole
                {
                    Id = 1,
                    Name = "AccountOfficer",
                    NormalizedName = "ACCOUNTOFFICER"
                },
                new ApplicationRole
                {
                    Id = 2,
                    Name = "ChiefAccountant",
                    NormalizedName = "CHIEFACCOUNTANT"
                },
                new ApplicationRole
                {
                    Id = 3,
                    Name = "Approval",
                    NormalizedName = "APPROVAL"
                },
                new ApplicationRole
                {
                    Id = 4,
                    Name = "Authorizer1",
                    NormalizedName = "AUTHORIZER1"
                },
                new ApplicationRole
                {
                    Id = 5,
                    Name = "Authorizer2",
                    NormalizedName = "AUTHORIZER2"
                },
                new ApplicationRole
                {
                    Id = 6,
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                }
                );
            //builder.Entity<ApplicationUser>().HasData(
            //    new ApplicationUser
            //    {
            //        Id = 7,
            //        Email = "admin@gmail.com",
            //        IsActive = true,
            //        UserName = "admin@gmail.com",
            //        FirstName = "Tega",
            //        LastName = "Odiete",
            //        PasswordHash = ""

            //    }) ;
        }
    }
}
