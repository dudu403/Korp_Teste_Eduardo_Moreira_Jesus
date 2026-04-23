using EstoqueService.Clients;
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
        private readonly FaturamentoClient _faturamentoClient;

        public ProdutoService(
            EstoqueDbContext context,
            IValidator<Produto> validator,
            FaturamentoClient faturamentoClient)
        {
            _context = context;
            _validator = validator;
            _faturamentoClient = faturamentoClient;
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

        public async Task<PagedResultDto<ProdutoResponseDto>> ListarPaginadoAsync(
            int page,
            int pageSize,
            string? search = null)
        {
            if (page < 1)
                page = 1;

            if (pageSize < 1)
                pageSize = 10;

            if (pageSize > 100)
                pageSize = 100;

            var query = _context.Produtos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(p =>
                    p.Codigo.ToLower().Contains(search) ||
                    p.Descricao.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Codigo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProdutoResponseDto
                {
                    Id = p.Id,
                    Codigo = p.Codigo,
                    Descricao = p.Descricao,
                    Saldo = p.Saldo
                })
                .ToListAsync();

            return new PagedResultDto<ProdutoResponseDto>
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
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

            var possuiNotaAberta = await _faturamentoClient.PossuiNotaAbertaPorProdutoAsync(produto.Codigo);

            if (possuiNotaAberta && !string.Equals(produto.Codigo, request.Codigo, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException("Não é possível alterar o código do produto, pois ele está vinculado a uma nota fiscal aberta.");

            if (possuiNotaAberta && request.Saldo < produto.Saldo)
            {
                var quantidadeEmNotasAbertas = await _faturamentoClient.ObterQuantidadeEmNotasAbertasAsync(produto.Codigo);

                if (request.Saldo < quantidadeEmNotasAbertas)
                    throw new ArgumentException("Não é possível reduzir o saldo do produto abaixo da quantidade vinculada em notas fiscais abertas.");
            }

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

            var possuiNotaAberta = await _faturamentoClient.PossuiNotaAbertaPorProdutoAsync(produto.Codigo);

            if (possuiNotaAberta)
                throw new ArgumentException("Não é possível excluir o produto, pois ele está vinculado a uma nota fiscal aberta.");

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