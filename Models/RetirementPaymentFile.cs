using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.Models
{
    public class RetirementPaymentFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(RetirementPayment))]
        public int RetirementPaymentId { get; set; }
        public RetirementPayment RetirementPayment { get; set; }
        public string Name { get; set; }
    }
}
