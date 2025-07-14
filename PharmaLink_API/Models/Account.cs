using Microsoft.AspNetCore.Identity;

namespace PharmaLink_API.Models
{
    public class Account : IdentityUser
    {
        //user-Account relationship (one to one)
        public User? User { get; set; }

        // User-Pharmacy relationship (one to one)
        public Pharmacy? Pharmacy { get; set; }
    }
}
