using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class CreateVoucherViewModel
    {
        public List<Particular> Particulars { get; set; }
        public List<Bank> Banks { get; set; }
    }
}
