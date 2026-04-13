using EstoqueService.Data;
using EstoqueService.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Domain.Validators
{
    public class ProdutoValidator : AbstractValidator<Produto>
    {
        private readonly EstoqueDbContext _context;

        public ProdutoValidator(EstoqueDbContext context)
        {
            _context = context;

            RuleFor(x => x.Codigo)
                .NotEmpty().WithMessage("O código não pode estar vazio.")
                .MaximumLength(20).WithMessage("O código deve ter no máximo 20 caracteres.")
                .MustAsync(async (produto, codigo, ct) =>
                {
                    var existe = await _context.Produtos
                        .AnyAsync(p => p.Codigo == codigo && p.Id != produto.Id, ct);

                    return !existe;
                }).WithMessage("Já existe um produto cadastrado com esse código.");

            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição não pode estar vazia.")
                .MaximumLength(100).WithMessage("A descrição deve ter no máximo 100 caracteres.");

            RuleFor(x => x.Saldo)
                .GreaterThanOrEqualTo(0).WithMessage("O saldo não pode ser negativo.");
        }
    }
}