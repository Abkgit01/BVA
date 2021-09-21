using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class EditViewModel
    {
        public ApplicationUser User { get; set; }
        public List<Department> Dept { get; set; }
        public string DeptName { get; set; }
        public string Email { get; set; }
    }
}
