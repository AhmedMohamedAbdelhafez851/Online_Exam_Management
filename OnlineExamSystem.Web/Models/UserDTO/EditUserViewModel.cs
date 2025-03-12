using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineExamSystem.Web.Models.UserDTO
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = "";

        [Required]
        public string FullName { get; set; } = "";

        [Required]
        public string Email { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";
    }
}
