using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Operacao;

public class ObterPorIdOperacaoUseCase
{
    private readonly IRepository<Contas_Db.Model.Operacao> _repository;

    public ObterPorIdOperacaoUseCase(IRepository<Contas_Db.Model.Operacao> repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Operacao?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
