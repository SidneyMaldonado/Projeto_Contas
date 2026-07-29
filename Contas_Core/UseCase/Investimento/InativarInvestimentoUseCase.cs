using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Investimento;

public class InativarInvestimentoUseCase
{
    private readonly IRepository<Contas_Db.Model.Investimento> _repository;

    public InativarInvestimentoUseCase(IRepository<Contas_Db.Model.Investimento> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.SoftDeleteAsync(id);
}
