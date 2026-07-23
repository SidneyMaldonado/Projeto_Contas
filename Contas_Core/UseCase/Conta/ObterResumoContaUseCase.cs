using Contas_Core.Dto;
using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Conta;

public class ObterResumoContaUseCase
{
    private readonly IRepository<Contas_Db.Model.Conta> _repository;

    public ObterResumoContaUseCase(IRepository<Contas_Db.Model.Conta> repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ContaResumoDto>> ExecuteAsync()
    {
        var contas = await _repository.GetAllAsync();

        return contas
            .Where(c => c.Ativo)
            .Select(c => new ContaResumoDto
            {
                Codigo = c.Id,
                Nome = c.Nome,
                Saldo = c.Saldo
            });
    }
}
