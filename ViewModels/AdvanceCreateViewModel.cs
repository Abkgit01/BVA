using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class AdvanceCreateViewModel
    {
        public List<Department> Dept { get; set; }
        public string userDept { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
