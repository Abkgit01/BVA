using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class RetirementFilesViewModel
    {
        public int UserId { get; set; }
        public List<RetirementPaymentFile> retirementPaymentFiles { get; set; }
    }
}
