using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.Models
{
    public class ApplicationFlow
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        //[ForeignKey(nameof(User))]
        //public int UserID { get; set; }
        //public ApplicationRole User { get; set; }

        //[ForeignKey(nameof(Role))]
        //public int RoleID { get; set; }
        //public ApplicationRole Role { get; set; }
        [ForeignKey(nameof(Dept))]
        public int DeptId { get; set; }
        public Department Dept { get; set; }
        public int Sort { get; set; }
        public bool Lead { get; set; }
        public bool isFinal { get; set; }
    }
}
