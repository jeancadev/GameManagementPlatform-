using FluentValidation;
using GameManagement.Application.DTOs;

namespace GameManagement.Application.Validators
{
    public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
    {
        public RegistrationRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("El nombre de usuario es requerido")
                .Length(3, 50).WithMessage("El nombre de usuario debe tener entre 3 y 50 caracteres")
                .Matches("^[a-zA-Z0-9_]*$").WithMessage("El nombre de usuario solo puede contener letras, números y guiones bajos");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("El email es requerido")
                .EmailAddress().WithMessage("El formato del email no es válido")
                .MaximumLength(100).WithMessage("El email no puede tener más de 100 caracteres");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("La contraseña es requerida")
                .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres")
                .Matches("[A-Z]").WithMessage("La contraseña debe contener al menos una letra mayúscula")
                .Matches("[a-z]").WithMessage("La contraseña debe contener al menos una letra minúscula")
                .Matches("[0-9]").WithMessage("La contraseña debe contener al menos un número")
                .Matches("[^a-zA-Z0-9]").WithMessage("La contraseña debe contener al menos un caracter especial");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden");
        }
    }
}