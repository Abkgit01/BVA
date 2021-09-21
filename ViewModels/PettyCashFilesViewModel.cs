using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class PettyCashFilesViewModel
    {
        public ApplicationUser User { get; set; }
        public List<PettyCashFile> Files { get; set; }
    }
}
