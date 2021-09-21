using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class CashAdvancePaymentsViewModel
    {
        
        public int Id { get; set; }
        public int CashAdvanceId { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string NumberInWords { get; set; }
        public List<CashAdvanceFile> cashAdvanceFiles { get; set; }
        public CashAdvance cashAdvance { get; set; }
        public List<CashAdvancePayment> cashAdvancePayments { get; set; }

        public IEnumerable<CashAdvanceAction> cashAdvanceActions { get; set; }
    }
}
