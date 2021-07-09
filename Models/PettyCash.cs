using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
namespace VoucherAutomationSystem.Models
{
    public class PettyCash
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Description { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        public ApplicationRole Role { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsApproved { get; set; }

        [ForeignKey(nameof(Dept))]
        public int DeptId { get; set; }
        public Department Dept { get; set; }
        public int CurrentStage { get; set; }
        public int UserToApprove { get; set; }
    }
}
