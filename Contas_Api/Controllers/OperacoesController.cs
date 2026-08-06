using Contas_Api.Extensions;
using Contas_Core.Converters;
using Contas_Contratos.Dto;
using Contas_Core.UseCase.Carteira;
using Contas_Core.UseCase.Investimento;
using Contas_Core.UseCase.Operacao;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperacoesController : ControllerBase
{
    private readonly AdicionarOperacaoUseCase _adicionar;
    private readonly AtualizarOperacaoUseCase _atualizar;
    private readonly ExcluirOperacaoUseCase _excluir;
    private readonly InativarOperacaoUseCase _inativar;
    private readonly ObterPorIdCarteiraUseCase _obterPorIdCarteira;
    private readonly ObterPorIdInvestimentoUseCase _obterPorIdInvestimento;
    private readonly ObterPorIdOperacaoUseCase _obterPorId;
    private readonly ObterTodosCarteiraUseCase _obterTodosCarteira;
    private readonly ObterTodosInvestimentoUseCase _obterTodosInvestimento;
    private readonly ObterTodosOperacaoUseCase _obterTodos;

    public OperacoesController(
        AdicionarOperacaoUseCase adicionar,
        AtualizarOperacaoUseCase atualizar,
        ExcluirOperacaoUseCase excluir,
        InativarOperacaoUseCase inativar,
        ObterPorIdCarteiraUseCase obterPorIdCarteira,
        ObterPorIdInvestimentoUseCase obterPorIdInvestimento,
        ObterPorIdOperacaoUseCase obterPorId,
        ObterTodosCarteiraUseCase obterTodosCarteira,
        ObterTodosInvestimentoUseCase obterTodosInvestimento,
        ObterTodosOperacaoUseCase obterTodos)
    {
        _adicionar = adicionar;
        _atualizar = atualizar;
        _excluir = excluir;
        _inativar = inativar;
        _obterPorIdCarteira = obterPorIdCarteira;
        _obterPorIdInvestimento = obterPorIdInvestimento;
        _obterPorId = obterPorId;
        _obterTodosCarteira = obterTodosCarteira;
        _obterTodosInvestimento = obterTodosInvestimento;
        _obterTodos = obterTodos;
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

        var entidades = (await _obterTodos.ExecuteAsync()).Where(o => meusInvestimentosIds.Contains(o.IdInvestimento));
        return Ok(OperacaoConverter.ToDto(entidades));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await InvestimentoPertenceAoUsuarioAsync(entidade.IdInvestimento, User.GetUsuarioId()))
            return NotFound();

        return Ok(OperacaoConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarOperacaoDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        if (!await InvestimentoPertenceAoUsuarioAsync(dto.IdInvestimento, usuarioId))
            return NotFound();

        var entidade = OperacaoConverter.ToEntity(dto);

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, OperacaoConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarOperacaoDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await InvestimentoPertenceAoUsuarioAsync(entidade.IdInvestimento, usuarioId))
            return NotFound();

        if (!await InvestimentoPertenceAoUsuarioAsync(dto.IdInvestimento, usuarioId))
            return NotFound();

        OperacaoConverter.ApplyUpdate(entidade, dto);

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
