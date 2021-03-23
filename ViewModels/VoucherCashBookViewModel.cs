using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class VoucherCashBookViewModel
    {
        public Voucher Voucher { get; set; }
        public List<CashBook> CashBooks { get; set; }

        public IEnumerable<Models.Action> Actions { get; set; }
        public int Id { get; set; }
        public List<Particular> Particulars { get; set; }
        public string Comment { get; set; }
        public int ActionPerformed { get; set; }
        public string RoleCreator { get; set; }
        public string NumberInWords { get; set; }
        public List<VoucherFile> VoucherFiles { get; set; }

    }
}
