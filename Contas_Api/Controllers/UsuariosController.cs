using Contas_Core.Converters;
using Contas_Core.Dto;
using Contas_Core.UseCase.Usuario;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AdicionarUsuarioUseCase _adicionar;
    private readonly AlterarSenhaUsuarioUseCase _alterarSenha;
    private readonly AtualizarUsuarioUseCase _atualizar;
    private readonly ExcluirUsuarioUseCase _excluir;
    private readonly InativarUsuarioUseCase _inativar;
    private readonly LoginUsuarioUseCase _login;
    private readonly ObterPorIdUsuarioUseCase _obterPorId;
    private readonly ObterTodosUsuarioUseCase _obterTodos;

    public UsuariosController(
        AdicionarUsuarioUseCase adicionar,
        AlterarSenhaUsuarioUseCase alterarSenha,
        AtualizarUsuarioUseCase atualizar,
        ExcluirUsuarioUseCase excluir,
        InativarUsuarioUseCase inativar,
        LoginUsuarioUseCase login,
        ObterPorIdUsuarioUseCase obterPorId,
        ObterTodosUsuarioUseCase obterTodos)
    {
        _adicionar = adicionar;
        _alterarSenha = alterarSenha;
        _atualizar = atualizar;
        _excluir = excluir;
        _inativar = inativar;
        _login = login;
        _obterPorId = obterPorId;
        _obterTodos = obterTodos;
    }

    [HttpGet]
    public async Task<IActionResult> ObterTodos()
    {
        var entidades = await _obterTodos.ExecuteAsync();
        return Ok(UsuarioConverter.ToDto(entidades));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        return entidade is null ? NotFound() : Ok(UsuarioConverter.ToDto(entidade));
    }

    [HttpPost]
    public async Task<IActionResult> Adicionar(AdicionarUsuarioDto dto)
    {
        var entidade = UsuarioConverter.ToEntity(dto);

        try
        {
            await _adicionar.ExecuteAsync(entidade);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = entidade.Id }, UsuarioConverter.ToDto(entidade));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, AtualizarUsuarioDto dto)
    {
        var entidade = await _obterPorId.ExecuteAsync(id);
        if (entidade is null)
            return NotFound();

        UsuarioConverter.ApplyUpdate(entidade, dto);

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

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUsuarioDto dto)
    {
        var entidade = await _login.ExecuteAsync(dto.Email, dto.Senha);
        return entidade is null ? Unauthorized() : Ok(UsuarioConverter.ToDto(entidade));
    }

    [HttpPatch("{id:int}/senha")]
    public async Task<IActionResult> AlterarSenha(int id, AlterarSenhaUsuarioDto dto)
    {
        var sucesso = await _alterarSenha.ExecuteAsync(id, dto.SenhaAtual, dto.NovaSenha);
        return sucesso ? NoContent() : BadRequest("Senha atual incorreta.");
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
