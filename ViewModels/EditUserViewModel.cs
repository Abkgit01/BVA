using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.ViewModels
{
    public class EditUserViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        public int DeptId { get; set; }
        public IFormFile Photo { get; set; }
        public bool RoleLead { get; set; }
    }
}
