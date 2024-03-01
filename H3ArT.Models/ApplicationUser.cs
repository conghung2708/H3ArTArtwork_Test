using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3ArT.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? AvatarImage {  get; set; }
        public bool Gender {  get; set; }
        public bool Status { get; set; } // Boolean property for Status

        [NotMapped]
        public string Role { get; set; }
    }
}
