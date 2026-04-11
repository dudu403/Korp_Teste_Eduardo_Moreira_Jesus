using FaturamentoService.Domain.Entities;
using FluentValidation;

namespace FaturamentoService.Domain.Validators
{
    public class NotaFiscalItemValidator : AbstractValidator<NotaFiscalItem>
    {
        public NotaFiscalItemValidator()
        {
            RuleFor(x => x.CodigoProduto)
                .NotEmpty()
                .WithMessage("O código do produto é obrigatório.");

            RuleFor(x => x.DescricaoProduto)
                .NotEmpty()
                .WithMessage("A descrição do produto é obrigatória.");

            RuleFor(x => x.Quantidade)
                .GreaterThan(0)
                .WithMessage("A quantidade deve ser maior que zero.");
        }
    }
}