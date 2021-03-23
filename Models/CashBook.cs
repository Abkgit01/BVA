using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.Models
{
    public class CashBook
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }

        [ForeignKey(nameof(Voucher))]
        public int VoucherId { get; set; }
        public Voucher Voucher { get; set; }
        public string Particular { get; set; }
    }
}
