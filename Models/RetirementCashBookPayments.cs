using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.Models
{
    public class RetirementCashBookPayments
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(RetirementPayment))]
        public int RetirePaymentId { get; set; }
        public RetirementPayment RetirementPayment { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
    }
}
