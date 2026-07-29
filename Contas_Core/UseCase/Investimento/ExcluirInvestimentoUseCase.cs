using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Investimento;

public class ExcluirInvestimentoUseCase
{
    private readonly IRepository<Contas_Db.Model.Investimento> _repository;

    public ExcluirInvestimentoUseCase(IRepository<Contas_Db.Model.Investimento> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
