using Contas_Api.Extensions;
using Contas_Core.Converters;
using Contas_Core.Dto;
using Contas_Core.UseCase.Carteira;
using Contas_Core.UseCase.Investimento;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InvestimentosController : ControllerBase
{
    private readonly AdicionarInvestimentoUseCase _adicionar;
    private readonly AtualizarInvestimentoUseCase _atualizar;
    private readonly ExcluirInvestimentoUseCase _excluir;
    private readonly InativarInvestimentoUseCase _inativar;
    private readonly ObterPorIdCarteiraUseCase _obterPorIdCarteira;
    private readonly ObterPorIdInvestimentoUseCase _obterPorId;
    private readonly ObterTodosCarteiraUseCase _obterTodosCarteira;
    private readonly ObterTodosInvestimentoUseCase _obterTodos;

    public InvestimentosController(
        AdicionarInvestimentoUseCase adicionar,
        AtualizarInvestimentoUseCase atualizar,
        ExcluirInvestimentoUseCase excluir,
        InativarInvestimentoUseCase inativar,
        ObterPorIdCarteiraUseCase obterPorIdCarteira,
        ObterPorIdInvestimentoUseCase obterPorId,
        ObterTodosCarteiraUseCase obterTodosCarteira,
        ObterTodosInvestimentoUseCase obterTodos)
    {
        _adicionar = adicionar;
        _atualizar = atualizar;
        _excluir = excluir;
        _inativar = inativar;
        _obterPorIdCarteira = obterPorIdCarteira;
        _obterPorId = obterPorId;
        _obterTodosCarteira = obterTodosCarteira;
        _obterTodos = obterTodos;
    }

    private async Task<bool> CarteiraPertenceAoUsuarioAsync(int idCarteira, int usuarioId)
    {
        var carteira = await _obterPorIdCarteira.ExecuteAsync(idCarteira);
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

        var entidades = (await _obterTodos.ExecuteAsync()).Where(i => minhasCarteirasIds.Contains(i.IdCarteira));
        return Ok(InvestimentoConverter.ToDto(entidades));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await CarteiraPertenceAoUsuarioAsync(entidade.IdCarteira, User.GetUsuarioId()))
            return NotFound();

        return Ok(InvestimentoConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarInvestimentoDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        if (!await CarteiraPertenceAoUsuarioAsync(dto.IdCarteira, usuarioId))
            return NotFound();

        var entidade = InvestimentoConverter.ToEntity(dto);

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, InvestimentoConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarInvestimentoDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await CarteiraPertenceAoUsuarioAsync(entidade.IdCarteira, usuarioId))
            return NotFound();

        if (!await CarteiraPertenceAoUsuarioAsync(dto.IdCarteira, usuarioId))
            return NotFound();

        InvestimentoConverter.ApplyUpdate(entidade, dto);

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
        if (entidade is null || !await CarteiraPertenceAoUsuarioAsync(entidade.IdCarteira, User.GetUsuarioId()))
            return NotFound();

        await _excluir.ExecuteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await CarteiraPertenceAoUsuarioAsync(entidade.IdCarteira, User.GetUsuarioId()))
            return NotFound();

        await _inativar.ExecuteAsync(id);
        return NoContent();
    }
}
