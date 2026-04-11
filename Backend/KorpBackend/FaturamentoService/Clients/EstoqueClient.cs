using System.Net.Http.Json;
using System.Text.Json;
using FaturamentoService.Clients.Dtos;

namespace FaturamentoService.Clients
{
    public class EstoqueClient
    {
        private readonly HttpClient _httpClient;

        public EstoqueClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ProdutoEstoqueResponseDto?> BuscarProdutoPorCodigoAsync(string codigo)
        {
            var response = await _httpClient.GetAsync($"api/produtos/codigo/{codigo}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<ProdutoEstoqueResponseDto>();
        }

        public async Task<ProdutoEstoqueResponseDto> BaixarEstoqueAsync(string codigo, int quantidade)
        {
            var request = new BaixarEstoqueRequestDto
            {
                Quantidade = quantidade
            };

            var response = await _httpClient.PutAsJsonAsync($"api/produtos/codigo/{codigo}/baixar-estoque", request);

            if (!response.IsSuccessStatusCode)
            {
                var conteudo = await response.Content.ReadAsStringAsync();

                try
                {
                    var erro = JsonSerializer.Deserialize<ErroResponseDto>(conteudo);

                    if (erro != null && !string.IsNullOrWhiteSpace(erro.Error))
                        throw new ArgumentException(erro.Error);
                }
                catch
                {
                }

                throw new ArgumentException("Erro ao baixar estoque.");
            }

            var produto = await response.Content.ReadFromJsonAsync<ProdutoEstoqueResponseDto>();

            if (produto == null)
                throw new ArgumentException("Não foi possível interpretar a resposta do serviço de estoque.");

            return produto;
        }
    }
}