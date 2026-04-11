using FaturamentoService.Domain.Entities;
using FluentValidation;

namespace FaturamentoService.Domain.Validators
{
    public class NotaFiscalValidator : AbstractValidator<NotaFiscal>
    {
        public NotaFiscalValidator()
        {
            RuleFor(x => x.Numero)
                .NotEmpty()
                .WithMessage("O número da nota fiscal é obrigatório.");

            RuleFor(x => x.DataEmissao)
                .NotEmpty()
                .WithMessage("A data de emissão é obrigatória.");

            RuleFor(x => x.Itens)
                .NotNull()
                .WithMessage("A nota fiscal deve possuir itens.")
                .Must(x => x.Any())
                .WithMessage("A nota fiscal deve possuir pelo menos um item.");

            RuleForEach(x => x.Itens)
                .SetValidator(new NotaFiscalItemValidator());
        }
    }
}