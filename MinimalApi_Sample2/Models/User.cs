using Microsoft.AspNetCore.Identity;

namespace MinimalApi_Sample2.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
