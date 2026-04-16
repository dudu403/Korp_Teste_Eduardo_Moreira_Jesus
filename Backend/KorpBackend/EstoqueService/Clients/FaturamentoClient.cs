using System.Net.Http.Json;
using EstoqueService.Clients.Dtos;

namespace EstoqueService.Clients
{
    public class FaturamentoClient
    {
        private readonly HttpClient _httpClient;

        public FaturamentoClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> PossuiNotaAbertaPorProdutoAsync(string codigoProduto)
        {
            var response = await _httpClient.GetAsync($"api/notasfiscais/produto/{codigoProduto}/possui-nota-aberta");

            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<PossuiNotaAbertaResponseDto>();

            if (resultado == null)
                throw new ArgumentException("Não foi possível interpretar a resposta do serviço de faturamento.");

            return resultado.PossuiNotaAberta;
        }

        public async Task<int> ObterQuantidadeEmNotasAbertasAsync(string codigoProduto)
        {
            var response = await _httpClient.GetAsync($"api/notasfiscais/produto/{codigoProduto}/quantidade-em-notas-abertas");

            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<QuantidadeEmNotasAbertasResponseDto>();

            if (resultado == null)
                throw new ArgumentException("Não foi possível interpretar a resposta do serviço de faturamento.");

            return resultado.Quantidade;
        }
    }
}