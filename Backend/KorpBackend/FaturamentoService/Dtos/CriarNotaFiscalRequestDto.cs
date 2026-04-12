namespace FaturamentoService.Dtos
{
    public class CriarNotaFiscalRequestDto
    {
        public string Numero { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }
        public List<NotaFiscalItemRequestDto> Itens { get; set; } = new();
    }
}