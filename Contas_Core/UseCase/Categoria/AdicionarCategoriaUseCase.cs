using Contas_Core.Biz;
using Contas_Db.Repository;

namespace Contas_Core.UseCase.Categoria;

public class AdicionarCategoriaUseCase
{
    private readonly IRepository<Contas_Db.Model.Categoria> _repository;

    public AdicionarCategoriaUseCase(IRepository<Contas_Db.Model.Categoria> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Categoria entity)
    {
        if (!new AdicionarCategoriaBiz().IsValid(entity))
            throw new ArgumentException("Categoria inválida: verifique o nome.");

        return _repository.AddAsync(entity);
    }
}
