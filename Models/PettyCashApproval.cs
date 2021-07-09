using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.Models
{
    public class PettyCashApproval
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? UserId { get; set; }

        [ForeignKey(nameof(petty))]
        public int PettyCashID { get; set; }
        public PettyCash petty { get; set; }
        public bool IsActive { get; set; }
    }
}
