using FluentValidation;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;

namespace PharmaLink_API.Infrastructure.Validators
{
    public class PharmacyProductDTOValidator : AbstractValidator<pharmacyProductDTO>
    {
        public PharmacyProductDTOValidator()
        {
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