using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class CompletePettyCashViewModel
    {
        public int Id { get; set; }
        public ApplicationUser User{ get; set; }
        public Department Dept { get; set; }
        public PettyCash PettyCash { get; set; }
        public List<PettyCashAction> pettyCashActions { get; set; }
        public List<PettyCashFile> pettyCashFiles { get; set; }
        public PettyCashApproval pettyCashApproval { get; set; }
        public string comment { get; set; }
        public int ActionPerformed { get; set; }
        public string NumberInWords { get; set; }
        public int CurrentUserId { get; set; }
    }
}
