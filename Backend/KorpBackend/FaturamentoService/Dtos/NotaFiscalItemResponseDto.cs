namespace FaturamentoService.Dtos
{
    public class NotaFiscalItemResponseDto
    {
        public Guid Id { get; set; }
        public string CodigoProduto { get; set; } = string.Empty;
        public string DescricaoProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }
}