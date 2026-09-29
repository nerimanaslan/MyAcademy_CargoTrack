using CargoTrack.DTO.DTOs.EmployeeDtos;
using CargoTrack.DTO.DTOs.TransferCenterDtos;
using FluentValidation;

namespace CargoTrack.Business.Validators.Operations
{
    public class CreateTransferCenterValidator : AbstractValidator<CreateTransferCenterDto>
    {
        public CreateTransferCenterValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Transfer merkezi adı boş bırakılamaz.")
                .MinimumLength(3).WithMessage("En az 3 karakter olmalıdır.");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Merkez kodu boş bırakılamaz.");

            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage("Şehir seçilmelidir.");
        }
    }

    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeDto>
    {
        public CreateEmployeeValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Ad boş bırakılamaz.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Soyad boş bırakılamaz.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Ünvan boş bırakılamaz.");
        }
    }
}
