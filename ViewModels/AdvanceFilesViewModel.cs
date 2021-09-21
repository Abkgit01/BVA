using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class AdvanceFilesViewModel
    {
        public int UserId { get; set; }
        public List<CashAdvanceFile> CashAdvanceFiles { get; set; }
    }
}
