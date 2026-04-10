namespace EstoqueService.Dtos
{
    public class CriarProdutoRequestDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Saldo { get; set; }
    }
}