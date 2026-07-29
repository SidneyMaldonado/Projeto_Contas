using Contas_Core.Converters;
using Contas_Core.Dto;
using Contas_Core.Security;
using Contas_Core.UseCase.Usuario;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Contas_Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly LoginUsuarioUseCase _login;
        private readonly JwtTokenGenerator _jwtTokenGenerator;
        public AuthController(JwtTokenGenerator jwtTokenGenerator, LoginUsuarioUseCase login)
        {
            _login = login;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUsuarioDto dto)
        {
            var entidade = await _login.ExecuteAsync(dto.Email, dto.Senha);
            if (entidade is null)
                return Unauthorized();

            var token = _jwtTokenGenerator.GenerateToken(entidade.Id, entidade.Email);
            return Ok(new LoginResponseDto { Token = token, Usuario = UsuarioConverter.ToDto(entidade) });
        }
        [AllowAnonymous]
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok("Running...");
        }
    }
}
