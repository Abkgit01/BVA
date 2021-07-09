using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class CreateDepartmentViewModel
    {
        [Required]
        [Display(Name = "Department")]
        public string DepartmentName { get; set; }
        public List<Department> Departments { get; set; }
    }
}
