using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineExamSystem.Web.Models.UserDTO
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Please Enter a Valid Email")]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "Please Enter a Valid Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        public bool RememberMe { get; set; }
    }
}
