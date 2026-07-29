using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Operacao;

public class ExcluirOperacaoUseCase
{
    private readonly IRepository<Contas_Db.Model.Operacao> _repository;

    public ExcluirOperacaoUseCase(IRepository<Contas_Db.Model.Operacao> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
