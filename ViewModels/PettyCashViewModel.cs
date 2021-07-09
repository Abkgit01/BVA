using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class PettyCashViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }
        public int RoleId { get; set; }
        public ApplicationRole Role { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsApproved { get; set; }
        public int DeptId { get; set; }
        public Department Dept { get; set; }
        public int CurrentStage { get; set; }
        public string Comment { get; set; }
        public string ApprovalUser { get; set; }
    }
}
