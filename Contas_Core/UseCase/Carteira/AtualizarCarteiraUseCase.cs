using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Carteira;

public class AtualizarCarteiraUseCase
{
    private readonly IRepository<Contas_Db.Model.Carteira> _repository;

    public AtualizarCarteiraUseCase(IRepository<Contas_Db.Model.Carteira> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Carteira entity) => _repository.UpdateAsync(entity);
}
