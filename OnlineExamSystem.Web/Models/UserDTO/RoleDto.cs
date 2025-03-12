using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineExamSystem.Web.Models.UserDTO
{
    public class RoleDto
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsAssigned { get; set; } // Indicates if the role is assigned to the user
    }
}
