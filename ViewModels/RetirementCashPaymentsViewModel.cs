using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class RetirementCashPaymentsViewModel
    {
        public int Id { get; set; }
        public int RetirementPaymentId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string NumberInWords { get; set; }
        public List<RetirementPaymentFile> retirementPaymentFiles { get; set; }
        public RetirementPayment retirementPayment { get; set; }
        public List<RetirementCashBookPayments> retirementCashBookPayments { get; set; }

        public IEnumerable<RetirementPaymentAction> retirementPaymentActions { get; set; }
    }
}
