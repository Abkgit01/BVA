using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class AllPettyCashViewModel
    {
        public List<PettyCashViewModel> pettyCashes { get; set; }
        public int TotalPendingVouchers { get; set; }
        public DateTime MinimumDate { get; set; }
        public DateTime MaximumDate { get; set; }
    }
}
