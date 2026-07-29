using Contas_Core.Biz;
using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Historico;

public class AdicionarHistoricoUseCase
{
    private readonly IRepository<Contas_Db.Model.Historico> _repository;

    public AdicionarHistoricoUseCase(IRepository<Contas_Db.Model.Historico> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Historico entity)
    {
        // Registro de auditoria imutável: garante DataHistorico preenchida mesmo que
        // a entidade tenha sido criada sem passar pelo HistoricoConverter.
        if (entity.DataHistorico == default)
            entity.DataHistorico = DateTime.UtcNow;

        if (!new AdicionarHistoricoBiz().IsValid(entity))
            throw new ArgumentException("Histórico inválido: verifique nome do investimento, quantidade, cotação e observação.");

        return _repository.AddAsync(entity);
    }
}
