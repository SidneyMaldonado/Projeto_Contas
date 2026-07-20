using Contas_Core.Biz;
using Contas_Db.Repository;

namespace Contas_Core.UseCase.Conta;

public class AdicionarContaUseCase
{
    private readonly IRepository<Contas_Db.Model.Conta> _repository;

    public AdicionarContaUseCase(IRepository<Contas_Db.Model.Conta> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Conta entity)
    {
        if (!new AdicionarContaBiz().IsValid(entity))
            throw new ArgumentException("Conta inválida: verifique o nome.");

        return _repository.AddAsync(entity);
    }
}
