namespace FaturamentoService.Dtos
{
    public class NotaFiscalResponseDto
    {
        public Guid Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }
        public string Status { get; set; } = string.Empty;
        public List<NotaFiscalItemResponseDto> Itens { get; set; } = new();
    }
}