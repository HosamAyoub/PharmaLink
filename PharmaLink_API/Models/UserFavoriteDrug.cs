namespace PharmaLink_API.Models
{
    public class UserFavoriteDrug
    {
        public int UserId { get; set; }
        public User? User { get; set; }

        public int DrugId { get; set; }
        public Drug? Drug { get; set; }
    }
}
