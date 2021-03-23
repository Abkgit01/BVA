using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoucherAutomationSystem.ViewModels
{
    public class EditRoleViewModel
    {
        public EditRoleViewModel()
        {
            Users = new List<string>();
        }

        public int Id { get; set; }

        public string RoleName { get; set; }
        public int? ParentRoleId { get; set; }
        public int? ChildRoleId { get; set; }

        public List<string> Users { get; set; }
    }
}
