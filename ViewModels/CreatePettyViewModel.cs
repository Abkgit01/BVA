using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class CreatePettyViewModel
    {
        public ApplicationUser User { get; set; }
        public Department Dept { get; set; }
    }
}
