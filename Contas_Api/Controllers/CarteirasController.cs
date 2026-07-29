using Contas_Api.Extensions;
using Contas_Core.Converters;
using Contas_Core.Dto;
using Contas_Core.UseCase.Carteira;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarteirasController : ControllerBase
{
    private readonly AdicionarCarteiraUseCase _adicionar;
    private readonly AtualizarCarteiraUseCase _atualizar;
    private readonly ExcluirCarteiraUseCase _excluir;
    private readonly InativarCarteiraUseCase _inativar;
    private readonly ObterPorIdCarteiraUseCase _obterPorId;
    private readonly ObterTodosCarteiraUseCase _obterTodos;

    public CarteirasController(
        AdicionarCarteiraUseCase adicionar,
        AtualizarCarteiraUseCase atualizar,
        ExcluirCarteiraUseCase excluir,
        InativarCarteiraUseCase inativar,
        ObterPorIdCarteiraUseCase obterPorId,
        ObterTodosCarteiraUseCase obterTodos)
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
        var usuarioId = User.GetUsuarioId();
        var entidades = (await _obterTodos.ExecuteAsync()).Where(c => c.IdUsuario == usuarioId);
        return Ok(CarteiraConverter.ToDto(entidades));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || entidade.IdUsuario != User.GetUsuarioId())
            return NotFound();

        return Ok(CarteiraConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarCarteiraDto dto)
    {
        var entidade = CarteiraConverter.ToEntity(dto);
        entidade.IdUsuario = User.GetUsuarioId();

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, CarteiraConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarCarteiraDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || entidade.IdUsuario != usuarioId)
            return NotFound();

        CarteiraConverter.ApplyUpdate(entidade, dto);
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
