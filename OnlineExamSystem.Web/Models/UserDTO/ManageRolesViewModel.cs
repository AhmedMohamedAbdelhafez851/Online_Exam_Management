using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineExamSystem.Web.Models.UserDTO
{
    public class ManageRolesViewModel
    {
        public string UserId { get; set; } = ""; // Add UserId property

        public string NewRoleName { get; set; } = "";
        public List<RoleDto>? Roles { get; set; } 
        public List<string>? UserRoles { get; set; } // New property to hold the user's roles
    }
}
