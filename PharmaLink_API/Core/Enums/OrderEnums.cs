namespace PharmaLink_API.Core.Enums
{
    public class SD
    {
        public const string StatusUnderReview = "UnderReview"; //10minutes
        public const string StatusReviewing = "Reviewing";     //15minutes
        public const string StatusPending = "Pending";         //30minutes
        public const string StatusOutForDelivery = "OutForDelivery";
        public const string StatusDelivered = "Delivered";    
        public const string StatusRejected = "Rejected";       //by pharmacy
        public const string StatusCancelled = "Cancelled";     //by customer


        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusRejected = "Rejected"; //by pharmacy
        public const string PaymentStatusRefunded = "Refunded";

    }
}