using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class RetirementCreateViewModel
    {
        public List<Department> Dept { get; set; }
        public string userDept { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public CashAdvance cashAdvance { get; set; }
        public List<CashAdvancePayment> cashAdvancePayments { get; set; }
    }
}
