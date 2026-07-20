using Contas_Db.Repository;

namespace Contas_Core.UseCase.Categoria;

public class AtualizarCategoriaUseCase
{
    private readonly IRepository<Contas_Db.Model.Categoria> _repository;

    public AtualizarCategoriaUseCase(IRepository<Contas_Db.Model.Categoria> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Categoria entity) => _repository.UpdateAsync(entity);
}
