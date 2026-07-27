using Contas_Api.Extensions;
using Contas_Core.Converters;
using Contas_Core.Dto;
using Contas_Core.UseCase.Conta;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ContasController : ControllerBase
{
    private readonly AdicionarContaUseCase _adicionar;
    private readonly AtualizarContaUseCase _atualizar;
    private readonly AtualizarSaldosContaUseCase _atualizarSaldos;
    private readonly ExcluirContaUseCase _excluir;
    private readonly InativarContaUseCase _inativar;
    private readonly ObterPorIdContaUseCase _obterPorId;
    private readonly ObterResumoContaUseCase _obterResumo;
    private readonly ObterTodosContaUseCase _obterTodos;

    public ContasController(
        AdicionarContaUseCase adicionar,
        AtualizarContaUseCase atualizar,
        AtualizarSaldosContaUseCase atualizarSaldos,
        ExcluirContaUseCase excluir,
        InativarContaUseCase inativar,
        ObterPorIdContaUseCase obterPorId,
        ObterResumoContaUseCase obterResumo,
        ObterTodosContaUseCase obterTodos)
    {
        _adicionar = adicionar;
        _atualizar = atualizar;
        _atualizarSaldos = atualizarSaldos;
        _excluir = excluir;
        _inativar = inativar;
        _obterPorId = obterPorId;
        _obterResumo = obterResumo;
        _obterTodos = obterTodos;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var usuarioId = User.GetUsuarioId();
        var entidades = (await _obterTodos.ExecuteAsync()).Where(c => c.IdUsuario == usuarioId);
        return Ok(ContaConverter.ToDto(entidades));
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> ObterResumo()
    {
        var resumo = await _obterResumo.ExecuteAsync(User.GetUsuarioId());
        return Ok(resumo);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || entidade.IdUsuario != User.GetUsuarioId())
            return NotFound();

        return Ok(ContaConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarContaDto dto)
    {
        var entidade = ContaConverter.ToEntity(dto);
        entidade.IdUsuario = User.GetUsuarioId();

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, ContaConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarContaDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || entidade.IdUsuario != usuarioId)
            return NotFound();

        ContaConverter.ApplyUpdate(entidade, dto);
        entidade.IdUsuario = usuarioId;

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

    [HttpPut("saldos")]
    public async Task<IActionResult> AtualizarSaldos(IEnumerable<ContaResumoDto> saldos)
    {
        var usuarioId = User.GetUsuarioId();
        var minhasContasIds = (await _obterTodos.ExecuteAsync())
            .Where(c => c.IdUsuario == usuarioId)
            .Select(c => c.Id)
            .ToHashSet();

        var saldosPermitidos = saldos.Where(s => minhasContasIds.Contains(s.Codigo));

        await _atualizarSaldos.ExecuteAsync(saldosPermitidos);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || entidade.IdUsuario != User.GetUsuarioId())
            return NotFound();

        await _excluir.ExecuteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || entidade.IdUsuario != User.GetUsuarioId())
            return NotFound();

        await _inativar.ExecuteAsync(id);
        return NoContent();
    }
}
