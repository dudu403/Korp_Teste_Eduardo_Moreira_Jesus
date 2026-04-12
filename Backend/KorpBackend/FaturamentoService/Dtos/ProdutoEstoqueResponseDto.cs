namespace FaturamentoService.Clients.Dtos
{
    public class ProdutoEstoqueResponseDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Saldo { get; set; }
    }
}