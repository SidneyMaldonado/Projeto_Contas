using Contas_Api.Extensions;
using Contas_Core.Converters;
using Contas_Contratos.Dto;
using Contas_Core.UseCase.Divida;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DividasController : ControllerBase
{
    private readonly AdicionarDividaUseCase _adicionar;
    private readonly AtualizarDividaUseCase _atualizar;
    private readonly ExcluirDividaUseCase _excluir;
    private readonly InativarDividaUseCase _inativar;
    private readonly ObterPorIdDividaUseCase _obterPorId;
    private readonly ObterTodosDividaUseCase _obterTodos;

    public DividasController(
        AdicionarDividaUseCase adicionar,
        AtualizarDividaUseCase atualizar,
        ExcluirDividaUseCase excluir,
        InativarDividaUseCase inativar,
        ObterPorIdDividaUseCase obterPorId,
        ObterTodosDividaUseCase obterTodos)
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
        var entidades = (await _obterTodos.ExecuteAsync()).Where(d => d.IdUsuario == usuarioId);
        return Ok(DividaConverter.ToDto(entidades));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || entidade.IdUsuario != User.GetUsuarioId())
            return NotFound();

        return Ok(DividaConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarDividaDto dto)
    {
        var entidade = DividaConverter.ToEntity(dto);
        entidade.IdUsuario = User.GetUsuarioId();

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, DividaConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarDividaDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || entidade.IdUsuario != usuarioId)
            return NotFound();

        DividaConverter.ApplyUpdate(entidade, dto);
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
