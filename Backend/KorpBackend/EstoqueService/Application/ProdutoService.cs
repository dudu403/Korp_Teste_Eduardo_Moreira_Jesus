using EstoqueService.Data;
using EstoqueService.Domain.Entities;
using EstoqueService.Dtos;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace EstoqueService.Application
{
    public class ProdutoService
    {
        private readonly EstoqueDbContext _context;
        private readonly IValidator<Produto> _validator;

        public ProdutoService(EstoqueDbContext context, IValidator<Produto> validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task<ProdutoResponseDto> CriarAsync(CriarProdutoRequestDto request)
        {
            var produto = new Produto
            {
                Codigo = request.Codigo,
                Descricao = request.Descricao,
                Saldo = request.Saldo
            };

            var result = await _validator.ValidateAsync(produto);
            if (!result.IsValid)
            {
                var mensagens = result.Errors.Select(e => e.ErrorMessage).ToList();
                throw new ArgumentException(string.Join("; ", mensagens));
            }

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return new ProdutoResponseDto
            {
                Id = produto.Id,
                Codigo = produto.Codigo,
                Descricao = produto.Descricao,
                Saldo = produto.Saldo
            };
        }

        public async Task<List<ProdutoResponseDto>> ListarTodosAsync()
        {
            return await _context.Produtos
                .Select(p => new ProdutoResponseDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Descricao = p.Descricao,
                    Saldo = p.Saldo
                })
                .ToListAsync();
        }

        public async Task<ProdutoResponseDto?> SelecionarPorIdAsync(Guid id)
        {
            return await _context.Produtos
                .Where(p => p.Id == id)
                .Select(p => new ProdutoResponseDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Descricao = p.Descricao,
                    Saldo = p.Saldo
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProdutoResponseDto> AtualizarAsync(Guid id, AtualizarProdutoRequestDto request)
        {
            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                throw new ArgumentException("Produto não encontrado.");

            produto.Codigo = request.Codigo;
            produto.Descricao = request.Descricao;
            produto.Saldo = request.Saldo;

            var result = await _validator.ValidateAsync(produto);
            if (!result.IsValid)
            {
                var mensagens = result.Errors.Select(e => e.ErrorMessage).ToList();
                throw new ArgumentException(string.Join("; ", mensagens));
            }

            await _context.SaveChangesAsync();

            return new ProdutoResponseDto
            {
                Id = produto.Id,
                Codigo = produto.Codigo,
                Descricao = produto.Descricao,
                Saldo = produto.Saldo
            };
        }

        public async Task ExcluirAsync(Guid id)
        {
            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id);

            if (produto == null)
                throw new ArgumentException("Produto não encontrado.");

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
        }

        public async Task<ProdutoResponseDto?> SelecionarPorCodigoAsync(string codigo)
        {
            return await _context.Produtos
                .Where(p => p.Codigo == codigo)
                .Select(p => new ProdutoResponseDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Descricao = p.Descricao,
                    Saldo = p.Saldo
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ProdutoResponseDto> BaixarEstoqueAsync(string codigo, int quantidade)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ArgumentException("O código do produto é obrigatório.");

            if (quantidade <= 0)
                throw new ArgumentException("A quantidade para baixa deve ser maior que zero.");

            var linhasAfetadas = await _context.Database.ExecuteSqlInterpolatedAsync($@"
                                                            UPDATE Produtos
                                                            SET Saldo = Saldo - {quantidade}
                                                            WHERE Codigo = {codigo} AND Saldo >= {quantidade}");

            if (linhasAfetadas == 0)
            {
                var produtoExiste = await _context.Produtos.AnyAsync(p => p.Codigo == codigo);

                if (!produtoExiste)
                    throw new ArgumentException("Produto não encontrado.");

                throw new ArgumentException("Saldo insuficiente para realizar a baixa.");
            }

            var produtoAtualizado = await _context.Produtos
                .Where(p => p.Codigo == codigo)
                .Select(p => new ProdutoResponseDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Descricao = p.Descricao,
                    Saldo = p.Saldo
                })
                .FirstAsync();

            return produtoAtualizado;
        }
    }
}