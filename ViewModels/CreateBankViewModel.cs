using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class CreateBankViewModel
    {
        [Required]
        [Display(Name = "Particular")]
        public string BankName { get; set; }
        public int BankId { get; set; }
        public List<Bank> bank { get; set; }
    }
}
