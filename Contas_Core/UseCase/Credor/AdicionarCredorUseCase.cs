using Contas_Core.Biz;
using Contas_Db.Repository;

namespace Contas_Core.UseCase.Credor;

public class AdicionarCredorUseCase
{
    private readonly IRepository<Contas_Db.Model.Credor> _repository;

    public AdicionarCredorUseCase(IRepository<Contas_Db.Model.Credor> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Credor entity)
    {
        if (!new AdicionarCredorBiz().IsValid(entity))
            throw new ArgumentException("Credor inválido: verifique o nome.");

        return _repository.AddAsync(entity);
    }
}
