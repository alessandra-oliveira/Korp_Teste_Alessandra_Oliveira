namespace Estoque.Api.Messaging;

public class AtualizarSaldoMensagem
{
    public int NotaFiscalId { get; set; }
    public List<ItemMensagem> Itens { get; set; } = new();
}

public class ItemMensagem
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
}