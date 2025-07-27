using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PharmaLink_API.Core.Attributes
{
    public class EgyptianPhoneNumberAttribute : ValidationAttribute
    {
        private const string Pattern = @"^(\+20|0)?1[0125][0-9]{8}$";

        public bool AcceptLandlines { get; set; } = false;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            string phone = value.ToString()!;

            if (string.IsNullOrWhiteSpace(phone))
                return ValidationResult.Success;

            if (Regex.IsMatch(phone, Pattern))
                return ValidationResult.Success;

            if (AcceptLandlines && IsValidLandline(phone))
                return ValidationResult.Success;

            return new ValidationResult(GetErrorMessage(phone));
        }

        private bool IsValidLandline(string phone)
        {
            return Regex.IsMatch(phone, @"^0[0-9]{9}$");
        }

        private string GetErrorMessage(string phone)
        {
            return AcceptLandlines
                ? $"Invalid Egyptian number. Mobile: 01[0,1,2,5]XXXXXXXX. Landline: 0[CityCode]XXXXXX (Received: {phone})"
                : $"Invalid Egyptian mobile number. Must start with 010/011/012/015 and be 11 digits (Received: {phone})";
        }
    }
}