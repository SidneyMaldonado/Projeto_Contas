using Contas_Core.Converters;
using Contas_Contratos.Dto;
using Contas_Core.UseCase.Categoria;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly AdicionarCategoriaUseCase _adicionar;
    private readonly AtualizarCategoriaUseCase _atualizar;
    private readonly ExcluirCategoriaUseCase _excluir;
    private readonly InativarCategoriaUseCase _inativar;
    private readonly ObterPorIdCategoriaUseCase _obterPorId;
    private readonly ObterTodosCategoriaUseCase _obterTodos;

    public CategoriasController(
        AdicionarCategoriaUseCase adicionar,
        AtualizarCategoriaUseCase atualizar,
        ExcluirCategoriaUseCase excluir,
        InativarCategoriaUseCase inativar,
        ObterPorIdCategoriaUseCase obterPorId,
        ObterTodosCategoriaUseCase obterTodos)
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
        return Ok(CategoriaConverter.ToDto(entidades));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        return entidade is null ? NotFound() : Ok(CategoriaConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarCategoriaDto dto)
    {
        var entidade = CategoriaConverter.ToEntity(dto);

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, CategoriaConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarCategoriaDto dto)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null)
            return NotFound();

        CategoriaConverter.ApplyUpdate(entidade, dto);

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
