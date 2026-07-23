using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Parcela;

public class ExcluirParcelaUseCase
{
    private readonly IParcelaRepository _repository;

    public ExcluirParcelaUseCase(IParcelaRepository repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
