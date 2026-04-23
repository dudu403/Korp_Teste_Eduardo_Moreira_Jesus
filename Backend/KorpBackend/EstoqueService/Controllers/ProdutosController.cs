using EstoqueService.Application;
using EstoqueService.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueService.Controllers
{
    [ApiController]
    [Route("api/produtos")]
    public class ProdutosController : ControllerBase
    {
        private readonly ProdutoService _produtoService;

        public ProdutosController(ProdutoService produtoService)
        {
            _produtoService = produtoService;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoResponseDto>> Criar([FromBody] CriarProdutoRequestDto request)
        {
            var produto = await _produtoService.CriarAsync(request);
            return CreatedAtAction(nameof(SelecionarPorId), new { id = produto.Id }, produto);
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ProdutoResponseDto>>> ListarTodos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var produtos = await _produtoService.ListarPaginadoAsync(page, pageSize, search);
            return Ok(produtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoResponseDto>> SelecionarPorId(Guid id)
        {
            var produto = await _produtoService.SelecionarPorIdAsync(id);

            if (produto == null)
                return NotFound(new { mensagem = "Produto não encontrado." });

            return Ok(produto);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, [FromBody] AtualizarProdutoRequestDto request)
        {
            try
            {
                var produtoAtualizado = await _produtoService.AtualizarAsync(id, request);
                return Ok(produtoAtualizado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Excluir(Guid id)
        {
            try
            {
                await _produtoService.ExcluirAsync(id);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("codigo/{codigo}")]
        public async Task<ActionResult<ProdutoResponseDto>> SelecionarPorCodigo(string codigo)
        {
            var produto = await _produtoService.SelecionarPorCodigoAsync(codigo);

            if (produto == null)
                return NotFound(new { error = "Produto não encontrado." });

            return Ok(produto);
        }

        [HttpPut("codigo/{codigo}/baixar-estoque")]
        public async Task<ActionResult<ProdutoResponseDto>> BaixarEstoque(string codigo, [FromBody] BaixarEstoqueRequestDto request)
        {
            var produto = await _produtoService.BaixarEstoqueAsync(codigo, request.Quantidade);
            return Ok(produto);
        }
    }
}