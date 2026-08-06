using Contas_Api.Extensions;
using Contas_Core.Converters;
using Contas_Contratos.Dto;
using Contas_Core.UseCase.Carteira;
using Contas_Core.UseCase.Historico;
using Contas_Core.UseCase.Investimento;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoricosController : ControllerBase
{
    private readonly AdicionarHistoricoUseCase _adicionar;
    private readonly AtualizarHistoricoUseCase _atualizar;
    private readonly ExcluirHistoricoUseCase _excluir;
    private readonly InativarHistoricoUseCase _inativar;
    private readonly ObterPorIdCarteiraUseCase _obterPorIdCarteira;
    private readonly ObterPorIdHistoricoUseCase _obterPorId;
    private readonly ObterPorIdInvestimentoUseCase _obterPorIdInvestimento;
    private readonly ObterTodosCarteiraUseCase _obterTodosCarteira;
    private readonly ObterTodosHistoricoUseCase _obterTodos;
    private readonly ObterTodosInvestimentoUseCase _obterTodosInvestimento;

    public HistoricosController(
        AdicionarHistoricoUseCase adicionar,
        AtualizarHistoricoUseCase atualizar,
        ExcluirHistoricoUseCase excluir,
        InativarHistoricoUseCase inativar,
        ObterPorIdCarteiraUseCase obterPorIdCarteira,
        ObterPorIdHistoricoUseCase obterPorId,
        ObterPorIdInvestimentoUseCase obterPorIdInvestimento,
        ObterTodosCarteiraUseCase obterTodosCarteira,
        ObterTodosHistoricoUseCase obterTodos,
        ObterTodosInvestimentoUseCase obterTodosInvestimento)
    {
        _adicionar = adicionar;
        _atualizar = atualizar;
        _excluir = excluir;
        _inativar = inativar;
        _obterPorIdCarteira = obterPorIdCarteira;
        _obterPorId = obterPorId;
        _obterPorIdInvestimento = obterPorIdInvestimento;
        _obterTodosCarteira = obterTodosCarteira;
        _obterTodos = obterTodos;
        _obterTodosInvestimento = obterTodosInvestimento;
    }

    private async Task<bool> InvestimentoPertenceAoUsuarioAsync(int idInvestimento, int usuarioId)
    {
        var investimento = await _obterPorIdInvestimento.ExecuteAsync(idInvestimento);
        if (investimento is null)
            return false;

        var carteira = await _obterPorIdCarteira.ExecuteAsync(investimento.IdCarteira);
        return carteira is not null && carteira.IdUsuario == usuarioId;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var usuarioId = User.GetUsuarioId();

        var minhasCarteirasIds = (await _obterTodosCarteira.ExecuteAsync())
            .Where(c => c.IdUsuario == usuarioId)
            .Select(c => c.Id)
            .ToHashSet();

        var meusInvestimentosIds = (await _obterTodosInvestimento.ExecuteAsync())
            .Where(i => minhasCarteirasIds.Contains(i.IdCarteira))
            .Select(i => i.Id)
            .ToHashSet();

        var entidades = (await _obterTodos.ExecuteAsync())
            .Where(h => meusInvestimentosIds.Contains(h.IdInvestimento));

        return Ok(HistoricoConverter.ToDto(entidades));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await InvestimentoPertenceAoUsuarioAsync(entidade.IdInvestimento, User.GetUsuarioId()))
            return NotFound();

        return Ok(HistoricoConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarHistoricoDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        if (!await InvestimentoPertenceAoUsuarioAsync(dto.IdInvestimento, usuarioId))
            return NotFound();

        var entidade = HistoricoConverter.ToEntity(dto);

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, HistoricoConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarHistoricoDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await InvestimentoPertenceAoUsuarioAsync(entidade.IdInvestimento, usuarioId))
            return NotFound();

        if (!await InvestimentoPertenceAoUsuarioAsync(dto.IdInvestimento, usuarioId))
            return NotFound();

        HistoricoConverter.ApplyUpdate(entidade, dto);

        try
        {
            await _atualizar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await InvestimentoPertenceAoUsuarioAsync(entidade.IdInvestimento, User.GetUsuarioId()))
            return NotFound();

        await _excluir.ExecuteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await InvestimentoPertenceAoUsuarioAsync(entidade.IdInvestimento, User.GetUsuarioId()))
            return NotFound();

        await _inativar.ExecuteAsync(id);
        return NoContent();
    }
}
