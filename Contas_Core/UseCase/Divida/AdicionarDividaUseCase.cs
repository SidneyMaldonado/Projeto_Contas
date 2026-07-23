using Contas_Core.Biz;
using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Divida;

public class AdicionarDividaUseCase
{
    private readonly IRepository<Contas_Db.Model.Divida> _repository;

    public AdicionarDividaUseCase(IRepository<Contas_Db.Model.Divida> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Divida entity)
    {
        if (!new AdicionarDividaBiz().IsValid(entity))
            throw new ArgumentException("Dívida inválida: verifique nome, valor, dia de vencimento e data de vencimento.");

        return _repository.AddAsync(entity);
    }
}
