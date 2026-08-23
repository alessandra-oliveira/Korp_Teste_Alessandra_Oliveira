using Faturamento.Api.Models;
using Faturamento.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Faturamento.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotasFiscaisController : ControllerBase
{
    private readonly NotaFiscalService _notaFiscalService;

    public NotasFiscaisController(NotaFiscalService notaFiscalService)
    {
        _notaFiscalService = notaFiscalService;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var notas = await _notaFiscalService.ListarAsync();
        return Ok(notas);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        try
        {
            var nota = await _notaFiscalService.ObterPorIdAsync(id);
            return Ok(nota);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] List<ItemNotaFiscal> itens)
    {
        try
        {
            var nota = await _notaFiscalService.CriarAsync(itens);
            return CreatedAtAction(nameof(ObterPorId), new { id = nota.Id }, nota);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ex.Message);
        }
    }

    [HttpPost("{id}/fechar")]
    public async Task<IActionResult> Fechar(int id)
    {
        try
        {
            await _notaFiscalService.FecharAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}