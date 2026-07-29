using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Investimento;

public class ObterPorIdInvestimentoUseCase
{
    private readonly IRepository<Contas_Db.Model.Investimento> _repository;

    public ObterPorIdInvestimentoUseCase(IRepository<Contas_Db.Model.Investimento> repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Investimento?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
