using Contas_Api.Extensions;
using Contas_Core.Converters;
using Contas_Core.Dto;
using Contas_Core.UseCase.Conta;
using Contas_Core.UseCase.Divida;
using Contas_Core.UseCase.Parcela;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ParcelasController : ControllerBase
{
    private readonly AdicionarParcelaUseCase _adicionar;
    private readonly AtualizarParcelaUseCase _atualizar;
    private readonly DesfazerPagamentoParcelaUseCase _desfazerPagamento;
    private readonly ExcluirParcelaUseCase _excluir;
    private readonly InativarParcelaUseCase _inativar;
    private readonly ObterPorIdContaUseCase _obterPorIdConta;
    private readonly ObterPorIdDividaUseCase _obterPorIdDivida;
    private readonly ObterPorIdParcelaUseCase _obterPorId;
    private readonly ObterTodosContaUseCase _obterTodosConta;
    private readonly ObterTodosParcelaUseCase _obterTodos;
    private readonly PagarParcelaUseCase _pagar;

    public ParcelasController(
        AdicionarParcelaUseCase adicionar,
        AtualizarParcelaUseCase atualizar,
        DesfazerPagamentoParcelaUseCase desfazerPagamento,
        ExcluirParcelaUseCase excluir,
        InativarParcelaUseCase inativar,
        ObterPorIdContaUseCase obterPorIdConta,
        ObterPorIdDividaUseCase obterPorIdDivida,
        ObterPorIdParcelaUseCase obterPorId,
        ObterTodosContaUseCase obterTodosConta,
        ObterTodosParcelaUseCase obterTodos,
        PagarParcelaUseCase pagar)
    {
        _adicionar = adicionar;
        _atualizar = atualizar;
        _desfazerPagamento = desfazerPagamento;
        _excluir = excluir;
        _inativar = inativar;
        _obterPorIdConta = obterPorIdConta;
        _obterPorIdDivida = obterPorIdDivida;
        _obterPorId = obterPorId;
        _obterTodosConta = obterTodosConta;
        _obterTodos = obterTodos;
        _pagar = pagar;
    }

    private async Task<bool> ContaPertenceAoUsuarioAsync(int idConta, int usuarioId)
    {
        var conta = await _obterPorIdConta.ExecuteAsync(idConta);
        return conta is not null && conta.IdUsuario == usuarioId;
    }

    private async Task<bool> DividaPertenceAoUsuarioAsync(int idDivida, int usuarioId)
    {
        var divida = await _obterPorIdDivida.ExecuteAsync(idDivida);
        return divida is not null && divida.IdUsuario == usuarioId;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var usuarioId = User.GetUsuarioId();
        var minhasContasIds = (await _obterTodosConta.ExecuteAsync())
            .Where(c => c.IdUsuario == usuarioId)
            .Select(c => c.Id)
            .ToHashSet();

        var entidades = (await _obterTodos.ExecuteAsync()).Where(p => minhasContasIds.Contains(p.IdConta));
        return Ok(ParcelaConverter.ToDto(entidades));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await ContaPertenceAoUsuarioAsync(entidade.IdConta, User.GetUsuarioId()))
            return NotFound();

        return Ok(ParcelaConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarParcelaDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        if (!await ContaPertenceAoUsuarioAsync(dto.IdConta, usuarioId) || !await DividaPertenceAoUsuarioAsync(dto.IdDivida, usuarioId))
            return NotFound();

        var entidade = ParcelaConverter.ToEntity(dto);

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, ParcelaConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarParcelaDto dto)
    {
        var usuarioId = User.GetUsuarioId();
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await ContaPertenceAoUsuarioAsync(entidade.IdConta, usuarioId))
            return NotFound();

        if (!await ContaPertenceAoUsuarioAsync(dto.IdConta, usuarioId) || !await DividaPertenceAoUsuarioAsync(dto.IdDivida, usuarioId))
            return NotFound();

        ParcelaConverter.ApplyUpdate(entidade, dto);

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

    [HttpPatch("{id:int}/pagar")]
    public async Task<IActionResult> Pagar(int id, PagarParcelaDto dto)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await ContaPertenceAoUsuarioAsync(entidade.IdConta, User.GetUsuarioId()))
            return NotFound();

        await _pagar.ExecuteAsync(id, dto.DataPagamento);
        return NoContent();
    }

    [HttpPatch("{id:int}/desfazer-pagamento")]
    public async Task<IActionResult> DesfazerPagamento(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await ContaPertenceAoUsuarioAsync(entidade.IdConta, User.GetUsuarioId()))
            return NotFound();

        await _desfazerPagamento.ExecuteAsync(id);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Excluir(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await ContaPertenceAoUsuarioAsync(entidade.IdConta, User.GetUsuarioId()))
            return NotFound();

        await _excluir.ExecuteAsync(id);
        return NoContent();
    }

    [HttpPatch("{id:int}/inativar")]
    public async Task<IActionResult> Inativar(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null || !await ContaPertenceAoUsuarioAsync(entidade.IdConta, User.GetUsuarioId()))
            return NotFound();

        await _inativar.ExecuteAsync(id);
        return NoContent();
    }
}
