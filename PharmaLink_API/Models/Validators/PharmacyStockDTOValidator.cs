using FluentValidation;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;

namespace PharmaLink_API.Models.Validators
{
    public class PharmacyStockDTOValidator : AbstractValidator<pharmacyProductDTO>
    {
        public PharmacyStockDTOValidator()
        {
            RuleFor(x => x.PharmacyId)
                .NotEmpty().WithMessage("Pharmacy ID is required.")
                .GreaterThan(0).WithMessage("Pharmacy ID must be greater than 0.");

            RuleFor(x => x.DrugId)
                .NotEmpty().WithMessage("Drug ID is required.")
                .GreaterThan(0).WithMessage("Drug ID must be greater than 0.");

            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Price is required.")
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x.QuantityAvailable)
                .NotEmpty().WithMessage("Quantity Available is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Quantity Available cannot be negative.");
        }
    }
}
