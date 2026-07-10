using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Parcela;

public class AdicionarParcelaUseCase
{
    private readonly IParcelaRepository _repository;

    public AdicionarParcelaUseCase(IParcelaRepository repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Parcela entity) => _repository.AddAsync(entity);
}
