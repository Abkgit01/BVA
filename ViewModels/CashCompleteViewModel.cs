using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class CashCompleteViewModel
    {
        public int Id { get; set; }
        public CashAdvance cashAdvance { get; set; }
        public List<Department> department { get; set; }
        public List<CashAdvancePayment> cashAdvancePayment { get; set; }
        public List<CashAdvanceAction> cashAdvanceActions { get; set; }
        public int ActionPerformed { get; set; }
        public string comment { get; set; }
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
