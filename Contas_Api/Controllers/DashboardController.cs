using Contas_Api.Extensions;
using Contas_Core.UseCase.Dashboard;
using Microsoft.AspNetCore.Mvc;

namespace Contas_Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ObterResumoDashboardUseCase _obterResumo;

    public DashboardController(ObterResumoDashboardUseCase obterResumo)
    {
        _obterResumo = obterResumo;
    }

    [HttpGet("resumo")]
    public async Task<IActionResult> ObterResumo()
    {
        var resumo = await _obterResumo.ExecuteAsync(User.GetUsuarioId());
        return Ok(resumo);
    }
}
