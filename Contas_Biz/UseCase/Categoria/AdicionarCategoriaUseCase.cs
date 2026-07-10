using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Categoria;

public class AdicionarCategoriaUseCase
{
    private readonly IRepository<Contas_Db.Model.Categoria> _repository;

    public AdicionarCategoriaUseCase(IRepository<Contas_Db.Model.Categoria> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Categoria entity) => _repository.AddAsync(entity);
}
