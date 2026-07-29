using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Investimento;

public class ObterTodosInvestimentoUseCase
{
    private readonly IRepository<Contas_Db.Model.Investimento> _repository;

    public ObterTodosInvestimentoUseCase(IRepository<Contas_Db.Model.Investimento> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Investimento>> ExecuteAsync() => _repository.GetAllAsync();
}
