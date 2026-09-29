using CargoTrack.DTO.DTOs.Cargos;
using FluentValidation;

namespace CargoTrack.Business.Validators.Cargos
{
    public class CreateCargoValidator : AbstractValidator<CreateCargoDto>
    {
        public CreateCargoValidator()
        {
            RuleFor(x => x.ReceiverName)
                .NotEmpty().WithMessage("Alıcı adı soyadı boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Alıcı adı en az 3 karakter olmalıdır.");

            RuleFor(x => x.ReceiverPhone)
                .NotEmpty().WithMessage("Alıcı telefon numarası boş bırakılamaz.")
                .Matches(@"^(05|5)[0-9]{9}$").WithMessage("Geçerli bir telefon numarası giriniz (örn: 05xxxxxxxxx).");

            RuleFor(x => x.ReceiverAddress)
                .NotEmpty().WithMessage("Teslimat adresi boş bırakılamaz.")
                .MinimumLength(10).WithMessage("Teslimat adresi en az 10 karakter olmalıdır.");

            RuleFor(x => x.OriginBranchId)
                .NotEmpty().WithMessage("Çıkış şubesi seçilmelidir.");

            RuleFor(x => x.DestinationBranchId)
                .NotEmpty().WithMessage("Varış şubesi seçilmelidir.")
                .NotEqual(x => x.OriginBranchId).WithMessage("Çıkış şubesi ile varış şubesi aynı olamaz.");

            RuleFor(x => x.Weight)
                .GreaterThan(0).WithMessage("Ağırlık 0'dan büyük olmalıdır.");

            RuleFor(x => x.Width)
                .GreaterThan(0).WithMessage("Genişlik 0'dan büyük olmalıdır.");

            RuleFor(x => x.Height)
                .GreaterThan(0).WithMessage("Yükseklik 0'dan büyük olmalıdır.");

            RuleFor(x => x.Length)
                .GreaterThan(0).WithMessage("Uzunluk 0'dan büyük olmalıdır.");
        }
    }

    public class VerifyDeliveryCodeValidator : AbstractValidator<VerifyDeliveryCodeDto>
    {
        public VerifyDeliveryCodeValidator()
        {
            RuleFor(x => x.CargoId)
                .NotEmpty().WithMessage("Kargo seçilmelidir.");

            RuleFor(x => x.DeliveryCode)
                .NotEmpty().WithMessage("Teslimat kodu boş bırakılamaz.")
                .Length(6).WithMessage("Teslimat kodu 6 haneli olmalıdır.")
                .Matches("^[0-9]{6}$").WithMessage("Teslimat kodu yalnızca rakamlardan oluşmalıdır.");

            RuleFor(x => x.ReceivedBy)
                .NotEmpty().WithMessage("Teslim alan kişi adı boş bırakılamaz.");
        }
    }

    public class CreateDeliveryExceptionValidator : AbstractValidator<CreateDeliveryExceptionDto>
    {
        public CreateDeliveryExceptionValidator()
        {
            RuleFor(x => x.CargoId)
                .NotEmpty().WithMessage("Kargo seçilmelidir.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Hata / Sorun nedeni seçilmelidir.");
        }
    }
}
