using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Divida;

public class AtualizarDividaUseCase
{
    private readonly IRepository<Contas_Db.Model.Divida> _repository;

    public AtualizarDividaUseCase(IRepository<Contas_Db.Model.Divida> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Divida entity) => _repository.UpdateAsync(entity);
}
