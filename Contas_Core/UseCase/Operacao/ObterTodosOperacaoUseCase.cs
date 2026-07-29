using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Operacao;

public class ObterTodosOperacaoUseCase
{
    private readonly IRepository<Contas_Db.Model.Operacao> _repository;

    public ObterTodosOperacaoUseCase(IRepository<Contas_Db.Model.Operacao> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Operacao>> ExecuteAsync() => _repository.GetAllAsync();
}
