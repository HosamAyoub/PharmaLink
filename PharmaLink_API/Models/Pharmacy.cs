namespace PharmaLink_API.Models
{
    public class Pharmacy
    {
        public int PharmacyID { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public double Rate { get; set; }
        public TimeOnly StartHour { get; set; }
        public TimeOnly EndHour { get; set; }

        //Pharmacy-Account relationship (one to one)
        public string AccountId { get; set; }
        public Account Account { get; set; }

        //Pharmacy-Drug relationship (many to many)
        public ICollection<PharmacyStock>? PharmacyStocks { get; set; }

        //Pharmacy-Order relationship (one to many)
        public ICollection<Order>? Orders { get; set; }
    }
}
