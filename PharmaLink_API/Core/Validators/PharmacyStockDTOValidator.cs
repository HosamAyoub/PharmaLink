using FluentValidation;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;

namespace PharmaLink_API.Core.Validators
{
    public class PharmacyStockDTOValidator : AbstractValidator<PharmacyStockDTO>
    {
        public PharmacyStockDTOValidator(IValidator<pharmacyProductDTO> productValidator)
        {
            RuleFor(x => x.Products)
                .NotEmpty().WithMessage("Products list cannot be empty.")
                .Must(products => products.Count > 0).WithMessage("At least one product must be provided.");
            
            RuleForEach(x => x.Products)
                .SetValidator(productValidator)
                .WithMessage("Each product must be valid.");
        }
    }
}