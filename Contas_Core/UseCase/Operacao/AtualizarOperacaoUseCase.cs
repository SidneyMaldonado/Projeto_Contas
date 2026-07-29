using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Operacao;

public class AtualizarOperacaoUseCase
{
    private readonly IRepository<Contas_Db.Model.Operacao> _repository;

    public AtualizarOperacaoUseCase(IRepository<Contas_Db.Model.Operacao> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Operacao entity) => _repository.UpdateAsync(entity);
}
