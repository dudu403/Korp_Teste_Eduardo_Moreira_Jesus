namespace FaturamentoService.Domain.Entities
{
    public class NotaFiscalItem : Entidade
    {
        public Guid NotaFiscalId { get; set; }
        public NotaFiscal? NotaFiscal { get; set; }

        public string CodigoProduto { get; set; } = string.Empty;
        public string DescricaoProduto { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }
}