using Contas_Core.Biz;
using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Parcela;

public class AdicionarParcelaUseCase
{
    private readonly IParcelaRepository _repository;

    public AdicionarParcelaUseCase(IParcelaRepository repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Parcela entity)
    {
        if (!new AdicionarParcelaBiz().IsValid(entity))
            throw new ArgumentException("Parcela inválida: verifique o valor.");

        return _repository.AddAsync(entity);
    }
}
