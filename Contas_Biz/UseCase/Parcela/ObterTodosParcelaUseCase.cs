using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Parcela;

public class ObterTodosParcelaUseCase
{
    private readonly IParcelaRepository _repository;

    public ObterTodosParcelaUseCase(IParcelaRepository repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Parcela>> ExecuteAsync() => _repository.GetAllAsync();
}
