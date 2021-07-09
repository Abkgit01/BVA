using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using VoucherAutomationSystem.Models.Enums;

namespace VoucherAutomationSystem.Models
{
    public class CashAdvanceAction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int CashAdvanceId { get; set; }
        public string Comment { get; set; }
        public ActionPerformed ActionPerformed { get; set; }

        [ForeignKey(nameof(User))]
        public int UserId { get; set; }
        public ApplicationUser User { get; set; }

        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        public ApplicationRole Role { get; set; }
        public DateTime DateUpdated { get; set; }
    }
}
