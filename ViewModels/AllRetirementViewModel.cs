using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class AllRetirementViewModel
    {
        public List<RetirementPayment> retirementPayments { get; set; }
        public DateTime MinimumDate { get; set; }
        public DateTime MaximumDate { get; set; }
    }
}
