using FaturamentoService.Clients;
using FaturamentoService.Data;
using FaturamentoService.Domain.Entities;
using FaturamentoService.Domain.Enums;
using FaturamentoService.Dtos;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FaturamentoService.Application
{
    public class NotaFiscalService
    {
        private readonly FaturamentoDbContext _context;
        private readonly IValidator<NotaFiscal> _validator;
        private readonly EstoqueClient _estoqueClient;

        public NotaFiscalService(
            FaturamentoDbContext context,
            IValidator<NotaFiscal> validator,
            EstoqueClient estoqueClient)
        {
            _context = context;
            _validator = validator;
            _estoqueClient = estoqueClient;
        }

        public async Task<NotaFiscalResponseDto> CriarAsync(CriarNotaFiscalRequestDto request)
        {
            var notaFiscal = new NotaFiscal
            {
                Numero = request.Numero,
                DataEmissao = request.DataEmissao,
                Status = StatusNotaFiscal.Aberta,
                Itens = request.Itens.Select(item => new NotaFiscalItem
                {
                    CodigoProduto = item.CodigoProduto,
                    DescricaoProduto = item.DescricaoProduto,
                    Quantidade = item.Quantidade
                }).ToList()
            };

            var result = await _validator.ValidateAsync(notaFiscal);
            if (!result.IsValid)
            {
                var mensagens = result.Errors.Select(e => e.ErrorMessage).ToList();
                throw new ArgumentException(string.Join("; ", mensagens));
            }

            _context.NotasFiscais.Add(notaFiscal);
            await _context.SaveChangesAsync();

            return new NotaFiscalResponseDto
            {
                Id = notaFiscal.Id,
                Numero = notaFiscal.Numero,
                DataEmissao = notaFiscal.DataEmissao,
                Status = notaFiscal.Status.ToString(),
                Itens = notaFiscal.Itens.Select(item => new NotaFiscalItemResponseDto
                {
                    Id = item.Id,
                    CodigoProduto = item.CodigoProduto,
                    DescricaoProduto = item.DescricaoProduto,
                    Quantidade = item.Quantidade
                }).ToList()
            };
        }

        public async Task<List<NotaFiscalResponseDto>> ListarTodosAsync()
        {
            return await _context.NotasFiscais
                .Include(n => n.Itens)
                .Select(notaFiscal => new NotaFiscalResponseDto
                {
                    Id = notaFiscal.Id,
                    Numero = notaFiscal.Numero,
                    DataEmissao = notaFiscal.DataEmissao,
                    Status = notaFiscal.Status.ToString(),
                    Itens = notaFiscal.Itens.Select(item => new NotaFiscalItemResponseDto
                    {
                        Id = item.Id,
                        CodigoProduto = item.CodigoProduto,
                        DescricaoProduto = item.DescricaoProduto,
                        Quantidade = item.Quantidade
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<NotaFiscalResponseDto?> SelecionarPorIdAsync(Guid id)
        {
            return await _context.NotasFiscais
                .Include(n => n.Itens)
                .Where(n => n.Id == id)
                .Select(notaFiscal => new NotaFiscalResponseDto
                {
                    Id = notaFiscal.Id,
                    Numero = notaFiscal.Numero,
                    DataEmissao = notaFiscal.DataEmissao,
                    Status = notaFiscal.Status.ToString(),
                    Itens = notaFiscal.Itens.Select(item => new NotaFiscalItemResponseDto
                    {
                        Id = item.Id,
                        CodigoProduto = item.CodigoProduto,
                        DescricaoProduto = item.DescricaoProduto,
                        Quantidade = item.Quantidade
                    }).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<NotaFiscalResponseDto> FecharAsync(Guid id)
        {
            var notaFiscal = await _context.NotasFiscais
                .Include(n => n.Itens)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (notaFiscal == null)
                throw new ArgumentException("Nota fiscal não encontrada.");

            if (notaFiscal.Status == StatusNotaFiscal.Fechada)
                throw new ArgumentException("A nota fiscal já está fechada.");

            foreach (var item in notaFiscal.Itens)
            {
                var produto = await _estoqueClient.BuscarProdutoPorCodigoAsync(item.CodigoProduto);

                if (produto == null)
                    throw new ArgumentException($"Produto com código {item.CodigoProduto} não encontrado no estoque.");

                await _estoqueClient.BaixarEstoqueAsync(item.CodigoProduto, item.Quantidade);
            }

            notaFiscal.Status = StatusNotaFiscal.Fechada;

            await _context.SaveChangesAsync();

            return new NotaFiscalResponseDto
            {
                Id = notaFiscal.Id,
                Numero = notaFiscal.Numero,
                DataEmissao = notaFiscal.DataEmissao,
                Status = notaFiscal.Status.ToString(),
                Itens = notaFiscal.Itens.Select(item => new NotaFiscalItemResponseDto
                {
                    Id = item.Id,
                    CodigoProduto = item.CodigoProduto,
                    DescricaoProduto = item.DescricaoProduto,
                    Quantidade = item.Quantidade
                }).ToList()
            };
        }

        public async Task<bool> PossuiNotaAbertaPorProdutoAsync(string codigoProduto)
        {
            return await _context.NotasFiscais
                .Include(n => n.Itens)
                .AnyAsync(n =>
                    n.Status == StatusNotaFiscal.Aberta &&
                    n.Itens.Any(i => i.CodigoProduto == codigoProduto));
        }

        public async Task<int> ObterQuantidadeEmNotasAbertasAsync(string codigoProduto)
        {
            return await _context.NotasFiscais
                .Where(n => n.Status == StatusNotaFiscal.Aberta)
                .SelectMany(n => n.Itens)
                .Where(i => i.CodigoProduto == codigoProduto)
                .SumAsync(i => (int?)i.Quantidade) ?? 0;
        }
    }
}