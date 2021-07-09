using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class RegViewModel
    {
        public List<Department> Depts { get; set; }
        public List<ApplicationRole> Roles { get; set; }
    }
}
