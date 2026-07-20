using Contas_Db.Repository;

namespace Contas_Core.UseCase.Parcela;

public class InativarParcelaUseCase
{
    private readonly IParcelaRepository _repository;

    public InativarParcelaUseCase(IParcelaRepository repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.SoftDeleteAsync(id);
}
