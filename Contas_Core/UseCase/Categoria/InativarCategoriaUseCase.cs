using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Categoria;

public class InativarCategoriaUseCase
{
    private readonly IRepository<Contas_Db.Model.Categoria> _repository;

    public InativarCategoriaUseCase(IRepository<Contas_Db.Model.Categoria> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.SoftDeleteAsync(id);
}
