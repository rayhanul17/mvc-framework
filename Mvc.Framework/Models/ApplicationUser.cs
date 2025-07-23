using Microsoft.AspNetCore.Identity;
using System;

namespace mvc.framework.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? Name { get; set; }
        public string? Image { get; set; }
        public DateTime? BirthOfDate { get; set; }
    }
}
