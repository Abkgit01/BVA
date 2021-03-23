using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models.Enums;

namespace VoucherAutomationSystem.Models
{
    public class Action
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey(nameof(Voucher))]
        public int VoucherId { get; set; }
        public Voucher Voucher { get; set; }
        public string Comment { get; set; }
        public ActionPerformed ActionPerformed { get; set; }
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey(nameof(ApplicationRole))]
        public int RoleId { get; set; }
        public ApplicationRole Role { get; set; }
        public DateTime DateUpdated { get; set; }
        public string UserName { get; set; }
    }
}
