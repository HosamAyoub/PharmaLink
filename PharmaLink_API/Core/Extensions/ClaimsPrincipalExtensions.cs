using System.Security.Claims;
using PharmaLink_API.Core.Constants;
using PharmaLink_API.Core.Enums;

namespace PharmaLink_API.Core.Extensions
{
    /// <summary>
    /// Extension methods for ClaimsPrincipal to easily access custom claims
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the pharmacy ID from user claims
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <returns>Pharmacy ID if found, null otherwise</returns>
        public static int? GetPharmacyId(this ClaimsPrincipal user)
        {
            var pharmacyIdClaim = user.FindFirst(CustomClaimTypes.PharmacyId)?.Value;
            return int.TryParse(pharmacyIdClaim, out int pharmacyId) ? pharmacyId : null;
        }

        /// <summary>
        /// Gets the patient ID from user claims
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <returns>Patient ID if found, null otherwise</returns>
        public static int? GetPatientId(this ClaimsPrincipal user)
        {
            var patientIdClaim = user.FindFirst(CustomClaimTypes.PatientId)?.Value;
            return int.TryParse(patientIdClaim, out int patientId) ? patientId : null;
        }


        /// <summary>
        /// Gets the patient name from user claims
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <returns>Patient name if found, null otherwise</returns>
        public static string? GetPatientName(this ClaimsPrincipal user)
        {
            return user.FindFirst(CustomClaimTypes.PatientName)?.Value;
        }

        /// <summary>
        /// Checks if the user has a specific role
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <param name="role">The UserRole to check</param>
        /// <returns>True if user has the role, false otherwise</returns>
        public static bool IsInRole(this ClaimsPrincipal user, UserRole role)
        {
            return user.IsInRole(role.ToRoleString());
        }

        /// <summary>
        /// Gets the user's role as UserRole enum
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <returns>UserRole if found, null otherwise</returns>
        public static UserRole? GetUserRole(this ClaimsPrincipal user)
        {
            var roleClaim = user.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleClaim))
                return null;

            try
            {
                return roleClaim.ToUserRole();
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if the user is a pharmacy user
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <returns>True if user is a pharmacy, false otherwise</returns>
        public static bool IsPharmacy(this ClaimsPrincipal user)
        {
            return user.IsInRole(UserRole.Pharmacy) && user.GetPharmacyId().HasValue;
        }

        /// <summary>
        /// Checks if the user is a patient user
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <returns>True if user is a patient, false otherwise</returns>
        public static bool IsPatient(this ClaimsPrincipal user)
        {
            return user.IsInRole(UserRole.Patient) && user.GetPatientId().HasValue;
        }

        /// <summary>
        /// Checks if the user is an admin
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <returns>True if user is an admin, false otherwise</returns>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.IsInRole(UserRole.Admin);
        }

        /// <summary>
        /// Gets the user ID (NameIdentifier claim)
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <returns>User ID if found, null otherwise</returns>
        public static string? GetUserId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// Gets the user email
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <returns>User email if found, null otherwise</returns>
        public static string? GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Gets a custom claim value by claim type
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user</param>
        /// <param name="claimType">The claim type to search for</param>
        /// <returns>Claim value if found, null otherwise</returns>
        public static string? GetCustomClaim(this ClaimsPrincipal user, string claimType)
        {
            return user.FindFirst(claimType)?.Value;
        }
    }
}