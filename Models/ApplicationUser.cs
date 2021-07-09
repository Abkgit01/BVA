using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.Models
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
        public string SignatureImage { get; set; }
        public bool RoleLead { get; set; }

        //[ForeignKey(nameof(Dept))]
        public int DeptId { get; set; }
        //public virtual Department Dept { get; set; }
        public bool IsActive { get; set; }
    }
}
