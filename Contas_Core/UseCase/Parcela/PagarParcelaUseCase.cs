using Contas_Db.Repository;

namespace Contas_Core.UseCase.Parcela;

public class PagarParcelaUseCase
{
    private readonly IParcelaRepository _repository;

    public PagarParcelaUseCase(IParcelaRepository repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id, DateTime dataPagamento) => _repository.PagarAsync(id, dataPagamento);
}
