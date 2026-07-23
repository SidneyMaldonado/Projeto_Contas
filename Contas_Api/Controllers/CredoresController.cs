using Contas_Core.Converters;
using Contas_Core.Dto;
using Contas_Core.UseCase.Credor;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CredoresController : ControllerBase
{
    private readonly AdicionarCredorUseCase _adicionar;
    private readonly AtualizarCredorUseCase _atualizar;
    private readonly ExcluirCredorUseCase _excluir;
    private readonly InativarCredorUseCase _inativar;
    private readonly ObterPorIdCredorUseCase _obterPorId;
    private readonly ObterTodosCredorUseCase _obterTodos;

    public CredoresController(
        AdicionarCredorUseCase adicionar,
        AtualizarCredorUseCase atualizar,
        ExcluirCredorUseCase excluir,
        InativarCredorUseCase inativar,
        ObterPorIdCredorUseCase obterPorId,
        ObterTodosCredorUseCase obterTodos)
    {
        _adicionar = adicionar;
        _atualizar = atualizar;
        _excluir = excluir;
        _inativar = inativar;
        _obterPorId = obterPorId;
        _obterTodos = obterTodos;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var entidades = await _obterTodos.ExecuteAsync();
        return Ok(CredorConverter.ToDto(entidades));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        return entidade is null ? NotFound() : Ok(CredorConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarCredorDto dto)
    {
        var entidade = CredorConverter.ToEntity(dto);

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, CredorConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarCredorDto dto)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null)
            return NotFound();

        CredorConverter.ApplyUpdate(entidade, dto);

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
        if (entidade is null)
            return NotFound();

        await _excluir.ExecuteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null)
            return NotFound();

        await _inativar.ExecuteAsync(id);
        return NoContent();
    }
}
