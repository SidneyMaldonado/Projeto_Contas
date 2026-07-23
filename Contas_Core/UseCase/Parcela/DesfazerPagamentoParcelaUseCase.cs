using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Parcela;

public class DesfazerPagamentoParcelaUseCase
{
    private readonly IParcelaRepository _repository;

    public DesfazerPagamentoParcelaUseCase(IParcelaRepository repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DesfazerPagamentoAsync(id);
}
