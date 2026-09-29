using CargoTrack.DTO.DTOs.CargoPriceDtos;
using FluentValidation;

namespace CargoTrack.Business.Validators.CargoPrices
{
    public class CalculatePriceValidator : AbstractValidator<CalculatePriceDto>
    {
        public CalculatePriceValidator()
        {
            RuleFor(x => x.OriginBranchId)
                .NotEmpty().WithMessage("Çıkış şubesi seçilmelidir.");

            RuleFor(x => x.DestinationBranchId)
                .NotEmpty().WithMessage("Varış şubesi seçilmelidir.")
                .NotEqual(x => x.OriginBranchId).WithMessage("Çıkış şubesi ile varış şubesi aynı olamaz.");

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Ağırlık 0'dan büyük olmalıdır.");

            RuleFor(x => x.Width)
                .GreaterThan(0).WithMessage("En 0'dan büyük olmalıdır.");

            RuleFor(x => x.Height)
                .GreaterThan(0).WithMessage("Yükseklik 0'dan büyük olmalıdır.");

            RuleFor(x => x.Length)
                .GreaterThan(0).WithMessage("Boy 0'dan büyük olmalıdır.");

            RuleFor(x => x.CargoType)
                .IsInEnum().WithMessage("Geçerli bir hizmet tipi seçilmelidir.");
        }
    }
}
