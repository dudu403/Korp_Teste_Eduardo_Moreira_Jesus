using FaturamentoService.Application;
using FaturamentoService.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FaturamentoService.Controllers
{
    [ApiController]
    [Route("api/notasfiscais")]
    public class NotasFiscaisController : ControllerBase
    {
        private readonly NotaFiscalService _notaFiscalService;

        public NotasFiscaisController(NotaFiscalService notaFiscalService)
        {
            _notaFiscalService = notaFiscalService;
        }

        [HttpPost]
        public async Task<ActionResult<NotaFiscalResponseDto>> Criar([FromBody] CriarNotaFiscalRequestDto request)
        {
            var notaFiscal = await _notaFiscalService.CriarAsync(request);
            return CreatedAtAction(nameof(SelecionarPorId), new { id = notaFiscal.Id }, notaFiscal);
        }

        [HttpGet]
        public async Task<ActionResult<List<NotaFiscalResponseDto>>> ListarTodos()
        {
            var notasFiscais = await _notaFiscalService.ListarTodosAsync();
            return Ok(notasFiscais);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<NotaFiscalResponseDto>> SelecionarPorId(Guid id)
        {
            var notaFiscal = await _notaFiscalService.SelecionarPorIdAsync(id);

            if (notaFiscal == null)
                return NotFound(new { mensagem = "Nota fiscal não encontrada." });

            return Ok(notaFiscal);
        }

        [HttpPut("{id:guid}/fechar")]
        public async Task<ActionResult<NotaFiscalResponseDto>> Fechar(Guid id)
        {
            var notaFiscal = await _notaFiscalService.FecharAsync(id);
            return Ok(notaFiscal);
        }

        [HttpGet("produto/{codigoProduto}/possui-nota-aberta")]
        public async Task<ActionResult<object>> PossuiNotaAbertaPorProduto(string codigoProduto)
        {
            var possuiNotaAberta = await _notaFiscalService.PossuiNotaAbertaPorProdutoAsync(codigoProduto);

            return Ok(new
            {
                possuiNotaAberta
            });
        }

        [HttpGet("produto/{codigoProduto}/quantidade-em-notas-abertas")]
        public async Task<ActionResult<object>> ObterQuantidadeEmNotasAbertas(string codigoProduto)
        {
            var quantidade = await _notaFiscalService.ObterQuantidadeEmNotasAbertasAsync(codigoProduto);

            return Ok(new
            {
                quantidade
            });
        }
    }
}