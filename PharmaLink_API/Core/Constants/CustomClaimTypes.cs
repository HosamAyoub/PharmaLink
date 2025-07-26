namespace PharmaLink_API.Core.Constants
{
    /// <summary>
    /// Contains custom claim type constants for PharmaLink application.
    /// Use these constants instead of hardcoded strings when working with claims.
    /// 
    /// EXAMPLE USAGE:
    /// 
    /// 1. In JWT Creation (AccountRepository):
    ///    claims.Add(new Claim(CustomClaimTypes.PharmacyId, pharmacy.PharmacyID.ToString()));
    /// 
    /// 2. In Service Methods:
    ///    var pharmacyId = user.GetPharmacyId(); // Using extension method
    ///    var isPharmacy = user.IsPharmacy();    // Using extension method
    ///    
    /// 3. Traditional Way (still supported):
    ///    var pharmacyIdClaim = user.FindFirst(CustomClaimTypes.PharmacyId)?.Value;
    /// 
    /// </summary>
    public static class CustomClaimTypes
    {
        /// <summary>
        /// Pharmacy ID claim type for pharmacy users
        /// </summary>
        public const string PharmacyId = "pharmacy_id";

        /// <summary>
        /// Patient ID claim type for patient users
        /// </summary>
        public const string PatientId = "patient_id";


        /// <summary>
        /// Account type claim (Patient, Pharmacy, Admin)
        /// </summary>
        public const string AccountType = "account_type";


        /// <summary>
        /// Patient name claim type
        /// </summary>
        public const string PatientName = "patient_name";
    }
}