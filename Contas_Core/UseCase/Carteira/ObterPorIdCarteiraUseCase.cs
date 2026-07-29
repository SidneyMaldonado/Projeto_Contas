using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Carteira;

public class ObterPorIdCarteiraUseCase
{
    private readonly IRepository<Contas_Db.Model.Carteira> _repository;

    public ObterPorIdCarteiraUseCase(IRepository<Contas_Db.Model.Carteira> repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Carteira?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
