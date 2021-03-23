using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models.Enums;

namespace VoucherAutomationSystem.ViewModels
{
    public class VoucherViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Payee { get; set; }
        public string AccountNo { get; set; }
        public string BankName { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsActive { get; set; }
        public VoucherType VoucherType { get; set; }
        public string Description { get; set; }
        public Currency Currency { get; set; }
        public int? ExchangeRate { get; set; }
        public string ChequeNo { get; set; }
        public string CurrentLevelRoleName { get; set; }
        public int? TotalAmount { get; set; }
        public string PhoneNo { get; set; }
        public string Address { get; set; }
        public string RoleCreator { get; set; }
        public string Comment { get; set; }
    }
}
