using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Categoria;

public class ExcluirCategoriaUseCase
{
    private readonly IRepository<Contas_Db.Model.Categoria> _repository;

    public ExcluirCategoriaUseCase(IRepository<Contas_Db.Model.Categoria> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
