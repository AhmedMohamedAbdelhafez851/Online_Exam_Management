using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineExamSystem.Web.Models.UserDTO
{
    public class EditUserRolesViewModel
    {
        public string UserId { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public IList<string>? UserRoles { get; set; } 
        public List<IdentityRole>? AllRoles { get; set; }
        public List<string>? SelectedRoles { get; set; }
    }
}
