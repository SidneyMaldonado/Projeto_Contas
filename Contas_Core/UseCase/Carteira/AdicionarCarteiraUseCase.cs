using Contas_Core.Biz;
using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Carteira;

public class AdicionarCarteiraUseCase
{
    private readonly IRepository<Contas_Db.Model.Carteira> _repository;

    public AdicionarCarteiraUseCase(IRepository<Contas_Db.Model.Carteira> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Carteira entity)
    {
        if (!new AdicionarCarteiraBiz().IsValid(entity))
            throw new ArgumentException("Carteira inválida: verifique o nome.");

        return _repository.AddAsync(entity);
    }
}
