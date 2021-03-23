using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.ViewModels
{
    public class VoucherModel
    {
        public VoucherViewModel vouchers { get; set; }
        public List<CashBookViewModel> vouchersCB { get; set; }
        public List<VoucherFileViewModel> files { get; set; }
    }
}
