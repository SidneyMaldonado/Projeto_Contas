using Contas_Api.Extensions;
using Contas_Core.Converters;
using Contas_Contratos.Dto;
using Contas_Core.UseCase.Conta;
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
    private readonly GerarParcelasDividaUseCase _gerarParcelas;
    private readonly InativarDividaUseCase _inativar;
    private readonly ObterPorIdContaUseCase _obterPorIdConta;
    private readonly ObterPorIdDividaUseCase _obterPorId;
    private readonly ObterTodosDividaUseCase _obterTodos;

    public DividasController(
        AdicionarDividaUseCase adicionar,
        AtualizarDividaUseCase atualizar,
        ExcluirDividaUseCase excluir,
        GerarParcelasDividaUseCase gerarParcelas,
        InativarDividaUseCase inativar,
        ObterPorIdContaUseCase obterPorIdConta,
        ObterPorIdDividaUseCase obterPorId,
        ObterTodosDividaUseCase obterTodos)
    {
        _adicionar = adicionar;
        _atualizar = atualizar;
        _excluir = excluir;
        _gerarParcelas = gerarParcelas;
        _inativar = inativar;
        _obterPorIdConta = obterPorIdConta;
        _obterPorId = obterPorId;
        _obterTodos = obterTodos;
    }

    private async Task<bool> ContaPertenceAoUsuarioAsync(int idConta, int usuarioId)
    {
        var conta = await _obterPorIdConta.ExecuteAsync(idConta);
        return conta is not null && conta.IdUsuario == usuarioId;
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
        var usuarioId = User.GetUsuarioId();
        if (!await ContaPertenceAoUsuarioAsync(dto.IdConta, usuarioId))
            return NotFound();

        var entidade = DividaConverter.ToEntity(dto);
        entidade.IdUsuario = usuarioId;

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        await _gerarParcelas.ExecuteAsync(entidade);

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
