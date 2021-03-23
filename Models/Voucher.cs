using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models.Enums;

namespace VoucherAutomationSystem.Models
{
    public class Voucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Payee { get; set; }
        public string AccountNo { get; set; }
        public string BankName { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsActive { get; set; }
        public VoucherType VoucherType { get; set; }
        public string Description { get; set; }
        public Currency Currency { get; set; }
        public int? ExchangeRate { get; set; }
        public string ChequeNo { get; set; }

        [ForeignKey(nameof(Role))]
        public string CurrentLevelRoleName { get; set; }
        public ApplicationRole Role { get; set; }
        public int? TotalAmount { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string RoleCreator { get; set; }



    }
}
