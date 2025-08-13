namespace PharmaLink_API.Core.Enums
{
    /// <summary>
    /// Enumeration representing the different user roles in the PharmaLink system
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Administrator role - has full access to the system
        /// </summary>
        Admin,

        /// <summary>
        /// Pharmacy role - can manage their own pharmacy stock and orders
        /// </summary>
        Pharmacy,

        /// <summary>
        /// Patient role - can browse pharmacies, place orders, and manage their profile
        /// Note: This maps to "User" in the database for legacy compatibility
        /// </summary>
        Patient,

        pending,
        suspended,
    }

    /// <summary>
    /// Extension methods for UserRole enum to provide string conversions
    /// </summary>
    public static class UserRoleExtensions
    {
        /// <summary>
        /// Converts UserRole enum to the corresponding string used in Identity system
        /// </summary>
        /// <param name="role">The UserRole enum value</param>
        /// <returns>String representation used by Identity system</returns>
        public static string ToRoleString(this UserRole role)
        {
            return role switch
            {
                UserRole.Admin => "Admin",
                UserRole.Pharmacy => "Pharmacy",
                UserRole.Patient => "Patient", // Legacy: Patient role is stored as "User" in database
                UserRole.pending => "pending",
                UserRole.suspended => "suspended",
                _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Invalid user role")
            };
        }

        /// <summary>
        /// Converts a role string to UserRole enum
        /// </summary>
        /// <param name="roleString">The role string from Identity system</param>
        /// <returns>Corresponding UserRole enum value</returns>
        /// <exception cref="ArgumentException">Thrown when role string is invalid</exception>
        public static UserRole ToUserRole(this string roleString)
        {
            return roleString switch
            {
                "Admin" => UserRole.Admin,
                "Pharmacy" => UserRole.Pharmacy,
                "Patient" => UserRole.Patient, // Legacy: "User" maps to Patient
                "pending" => UserRole.pending,
                "suspended" => UserRole.suspended,
                _ => throw new ArgumentException($"Invalid role string: {roleString}", nameof(roleString))
            };
        }

        /// <summary>
        /// Gets all available role strings for Identity system
        /// </summary>
        /// <returns>Array of all role strings</returns>
        public static string[] GetAllRoleStrings()
        {
            return new[] { "Admin", "Pharmacy", "Patient", "pending", "suspended"};
        }

        /// <summary>
        /// Gets all UserRole enum values
        /// </summary>
        /// <returns>Array of all UserRole values</returns>
        public static UserRole[] GetAllRoles()
        {
            return Enum.GetValues<UserRole>();
        }
    }
}