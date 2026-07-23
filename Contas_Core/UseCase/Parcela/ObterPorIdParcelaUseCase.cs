using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Parcela;

public class ObterPorIdParcelaUseCase
{
    private readonly IParcelaRepository _repository;

    public ObterPorIdParcelaUseCase(IParcelaRepository repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Parcela?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
