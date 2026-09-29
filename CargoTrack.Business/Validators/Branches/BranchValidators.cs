using CargoTrack.DTO.DTOs.BranchDtos;
using FluentValidation;

namespace CargoTrack.Business.Validators.Branches
{
    public class CreateBranchValidator : AbstractValidator<CreateBranchDto>
    {
        public CreateBranchValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Şube adı boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Şube adı en az 3 karakter olmalıdır.");

            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage("Şehir seçilmelidir.");
        }
    }

    public class UpdateBranchValidator : AbstractValidator<UpdateBranchDto>
    {
        public UpdateBranchValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Şube ID gereklidir.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Şube adı boş bırakılamaz.")
                .MinimumLength(3).WithMessage("Şube adı en az 3 karakter olmalıdır.");

            RuleFor(x => x.CityId)
                .NotEmpty().WithMessage("Şehir seçilmelidir.");
        }
    }
}
