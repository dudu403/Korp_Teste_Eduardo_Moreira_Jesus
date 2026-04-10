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
        public async Task<ActionResult<List<ProdutoResponseDto>>> ListarTodos()
        {
            var produtos = await _produtoService.ListarTodosAsync();
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
    }
}