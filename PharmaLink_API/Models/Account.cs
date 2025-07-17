using Microsoft.AspNetCore.Identity;

namespace PharmaLink_API.Models
{
    public class Account : IdentityUser
    {
        // Represents a user account in the PharmaLink system, inheriting from IdentityUser.
        public string DisplayName { get; set; }
        //user-Account relationship (one to one)
        public Patient? Patient { get; set; }

        // User-Pharmacy relationship (one to one)
        public Pharmacy? Pharmacy { get; set; }
    }
}
