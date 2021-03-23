using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.ViewModels
{
    public class VoucherFileViewModel
    {

        public List<string> Name { get; set; }
        public List<IFormFile> File { get; set; }
        public int VoucherId { get; set; }
        
        
    }
}
