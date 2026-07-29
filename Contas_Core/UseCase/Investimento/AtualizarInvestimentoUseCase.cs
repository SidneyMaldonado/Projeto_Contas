using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Investimento;

public class AtualizarInvestimentoUseCase
{
    private readonly IRepository<Contas_Db.Model.Investimento> _repository;

    public AtualizarInvestimentoUseCase(IRepository<Contas_Db.Model.Investimento> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Investimento entity) => _repository.UpdateAsync(entity);
}
