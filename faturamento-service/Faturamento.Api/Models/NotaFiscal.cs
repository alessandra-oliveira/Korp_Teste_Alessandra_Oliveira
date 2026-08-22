namespace Faturamento.Api.Models;

public class NotaFiscal
{
    public int Id { get; set; }
    public int Numero { get; set; }
    public StatusNotaFiscal Status { get; set; } = StatusNotaFiscal.Aberta;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public List<ItemNotaFiscal> Itens { get; set; } = new();
}

public enum StatusNotaFiscal
{
    Aberta,
    Fechada
}