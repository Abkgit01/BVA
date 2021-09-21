using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models.Enums;

namespace VoucherAutomationSystem.ViewModels
{
    public class CashAdvanceViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public decimal TotalAmount { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public int DeptId { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool IsApproved { get; set; }
        public string CurrentLevelRole { get; set; }
        public Currency Currency { get; set; }
        public string Comment { get; set; }
        public int ExchangeRate { get; set; }
    }
}
