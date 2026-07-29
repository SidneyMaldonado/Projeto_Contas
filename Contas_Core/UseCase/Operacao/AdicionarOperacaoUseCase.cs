using Contas_Core.Biz;
using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Operacao;

public class AdicionarOperacaoUseCase
{
    private readonly IRepository<Contas_Db.Model.Operacao> _repository;

    public AdicionarOperacaoUseCase(IRepository<Contas_Db.Model.Operacao> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Operacao entity)
    {
        if (!new AdicionarOperacaoBiz().IsValid(entity))
            throw new ArgumentException("Operação inválida: verifique quantidade, valor e data.");

        return _repository.AddAsync(entity);
    }
}
