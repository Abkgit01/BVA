using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models;

namespace VoucherAutomationSystem.ViewModels
{
    public class CreateParticularViewModel
    {
        [Required]
        [Display(Name = "Particular")]
        public string ParticularName { get; set; }
        public List<Particular> particular { get; set; }
    }
}
