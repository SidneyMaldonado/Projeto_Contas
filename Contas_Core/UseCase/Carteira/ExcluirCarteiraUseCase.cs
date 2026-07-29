using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Carteira;

public class ExcluirCarteiraUseCase
{
    private readonly IRepository<Contas_Db.Model.Carteira> _repository;

    public ExcluirCarteiraUseCase(IRepository<Contas_Db.Model.Carteira> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
