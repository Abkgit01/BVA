using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.ViewModels
{
    public class PettyCashBookViewModel
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public int PettyCashId { get; set; }
    }
}
