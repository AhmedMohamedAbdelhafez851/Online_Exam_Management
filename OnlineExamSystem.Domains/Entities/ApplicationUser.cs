using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Users
namespace OnlineExamSystem.Domains.Entities
{
    
    public class ApplicationUser : IdentityUser
    {
        // Add any custom properties you want here, such as FullName, etc.
        public string FullName { get; set; } = "";


    }
}
