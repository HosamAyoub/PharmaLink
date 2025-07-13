namespace PharmaLink_API.Models
{
    public class Account
    {
        public Guid AccountID { get; set; }
        public string Role { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        //user-Account relationship (one to one)
        public User? User { get; set; }

        // User-Pharmacy relationship (one to one)
        public Pharmacy? Pharmacy { get; set; }
    }
}
