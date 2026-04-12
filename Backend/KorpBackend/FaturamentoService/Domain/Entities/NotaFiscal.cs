using FaturamentoService.Domain.Enums;

namespace FaturamentoService.Domain.Entities
{
    public class NotaFiscal : Entidade
    {
        public string Numero { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }
        public StatusNotaFiscal Status { get; set; } = StatusNotaFiscal.Aberta;
        public List<NotaFiscalItem> Itens { get; set; } = new();
    }
}