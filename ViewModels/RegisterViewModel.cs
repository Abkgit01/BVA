﻿using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.ViewModels
{
    public class RegisterViewModel
    {

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
       
        public IFormFile Photo { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public int DeptId { get; set; }
        public bool RoleLead { get; set; }
    }
}
