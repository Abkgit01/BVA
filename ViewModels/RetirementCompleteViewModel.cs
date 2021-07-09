using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class RetirementCompleteViewModel
    {
        public int Id { get; set; }
        public RetirementPayment retirementPayment { get; set; }
        public List<Department> departments { get; set; }
        public List<RetirementCashBookPayments> retirementCashBookPayments { get; set; }
        public List<RetirementPaymentAction> retirementPaymentActions { get; set; }
        public CashAdvance cashAdvance { get; set; }
        public List<CashAdvancePayment> cashAdvancePayments { get; set; }
        public int ActionPerformed { get; set; }
        public string comment { get; set; }
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }
        public string NumberInWords { get; set; }
        public List<RetirementPaymentFile> retirementPaymentFiles { get; set; }

        public decimal? retireCash { get; set; }
    }
}
