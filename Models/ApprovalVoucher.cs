using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.Models
{
    public class ApprovalVoucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? UserId { get; set; }

        [ForeignKey(nameof(voucher))]
        public int VoucherId { get; set; }
        public Voucher voucher { get; set; }
        public bool IsActive { get; set; }
    }

}
