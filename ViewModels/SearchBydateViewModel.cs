using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.ViewModels
{
    public class SearchBydateViewModel
    {
        public DateTime MinimumDate { get; set; }
        public DateTime MaximumDate { get; set; }
    }
}
