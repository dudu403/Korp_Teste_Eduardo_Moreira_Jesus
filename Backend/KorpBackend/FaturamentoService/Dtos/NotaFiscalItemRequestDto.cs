namespace FaturamentoService.Dtos
{
    public class NotaFiscalItemRequestDto
    {
        public string CodigoProduto { get; set; } = string.Empty;
        public string DescricaoProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }
}