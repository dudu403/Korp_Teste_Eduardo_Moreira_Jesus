namespace EstoqueService.Domain.Entities
{
    public class Produto : Entidade
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Saldo { get; set; }
    }
}