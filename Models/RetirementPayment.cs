using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models.Enums;

namespace VoucherAutomationSystem.Models
{
    public class RetirementPayment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal? TotalRetirementAmount { get; set; }
        public decimal? CashAdvanceAmount { get; set; }
        public bool IsCredit { get; set; }
        public string Description { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        
        public int RoleId { get; set; }
        
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsApproved { get; set; }
        public int ExchangeRate { get; set; }
        public int CashAdvanceId { get; set; }

        [ForeignKey(nameof(Dept))]
        public int DeptId { get; set; }
        public Department Dept { get; set; }
        public Currency Currency { get; set; }
        public int CurrentStage { get; set; }
    }
}
