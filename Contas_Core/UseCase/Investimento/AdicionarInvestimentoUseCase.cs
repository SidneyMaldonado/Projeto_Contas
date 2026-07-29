using Contas_Core.Biz;
using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Investimento;

public class AdicionarInvestimentoUseCase
{
    private readonly IRepository<Contas_Db.Model.Investimento> _repository;

    public AdicionarInvestimentoUseCase(IRepository<Contas_Db.Model.Investimento> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Investimento entity)
    {
        if (!new AdicionarInvestimentoBiz().IsValid(entity))
            throw new ArgumentException("Investimento inválido: verifique nome, quantidade e cotação.");

        return _repository.AddAsync(entity);
    }
}
