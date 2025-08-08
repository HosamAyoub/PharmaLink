using FluentValidation;
using PharmaLink_API.Models.DTO.PharmacyStockDTO;

namespace PharmaLink_API.Core.Validators
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
                .GreaterThanOrEqualTo(0).WithMessage("Quantity Available cannot be negative.");
        }
    }

    public class UpdateQuantityDTOValidator : AbstractValidator<UpdateQuantityDTO>
    {
        public UpdateQuantityDTOValidator()
        {
            RuleFor(x => x.DrugId)
                .NotEmpty().WithMessage("Drug ID is required.")
                .GreaterThan(0).WithMessage("Drug ID must be greater than 0.");

            RuleFor(x => x.QuantityAvailable)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity Available cannot be negative.");
        }
    }

    public class UpdatePriceOnlyDTOValidator : AbstractValidator<UpdatePriceOnlyDTO>
    {
        public UpdatePriceOnlyDTOValidator()
        {
            RuleFor(x => x.Price)
                .NotEmpty().WithMessage("Price is required.")
                .GreaterThan(0).WithMessage("Price must be greater than 0.");
        }
    }

    public class IncreaseQuantityDTOValidator : AbstractValidator<IncreaseQuantityDTO>
    {
        public IncreaseQuantityDTOValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity to increase must be a positive number.");
        }
    }

    public class DecreaseQuantityDTOValidator : AbstractValidator<DecreaseQuantityDTO>
    {
        public DecreaseQuantityDTOValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity to decrease must be a positive number.");
        }
    }
}