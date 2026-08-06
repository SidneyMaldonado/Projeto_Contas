using Contas_Contratos.Dto;
using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Conta;

public class AtualizarSaldosContaUseCase
{
    private readonly IContaRepository _repository;

    public AtualizarSaldosContaUseCase(IContaRepository repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(IEnumerable<ContaResumoDto> contas) =>
        _repository.AtualizarSaldosAsync(contas.Select(c => (c.Codigo, c.Saldo)));
}
