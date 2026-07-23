using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Parcela;

public class AtualizarParcelaUseCase
{
    private readonly IParcelaRepository _repository;

    public AtualizarParcelaUseCase(IParcelaRepository repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Parcela entity) => _repository.UpdateAsync(entity);
}
