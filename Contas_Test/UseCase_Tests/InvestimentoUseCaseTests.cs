using Contas_Core.UseCase.Investimento;
using Contas_Db.Model;
using Contas_Db.Repository;
using Contas_Db.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.UseCase_Tests
{
    [TestClass]
    public sealed class InvestimentoUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IRepository<Investimento> _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new Repository<Investimento>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Investimento CriarInvestimento(
            string nome = "Tesouro Selic",
            decimal quantidade = 10m,
            decimal cotacao = 100m,
            int idCarteira = 1) => new()
        {
            IdCarteira = idCarteira,
            Nome = nome,
            Quantidade = quantidade,
            Cotacao = cotacao,
            Ativo = true
        };

        [TestMethod]
        public async Task AdicionarInvestimentoUseCase_DeveAdicionarInvestimento()
        {
            var useCase = new AdicionarInvestimentoUseCase(_repository);
            var investimento = CriarInvestimento();

            await useCase.ExecuteAsync(investimento);

            Assert.AreEqual(1, await _context.Investimentos.CountAsync());
            Assert.AreNotEqual(0, investimento.Id);
        }

        [TestMethod]
        public async Task AdicionarInvestimentoUseCase_DeveLancarExcecao_QuandoNomeVazio()
        {
            var useCase = new AdicionarInvestimentoUseCase(_repository);
            var investimento = CriarInvestimento(nome: "");

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(investimento));
        }

        [TestMethod]
        public async Task AdicionarInvestimentoUseCase_DeveLancarExcecao_QuandoNomeMaiorQue50Caracteres()
        {
            var useCase = new AdicionarInvestimentoUseCase(_repository);
            var investimento = CriarInvestimento(nome: new string('A', 51));

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(investimento));
        }

        [TestMethod]
        public async Task AdicionarInvestimentoUseCase_DeveLancarExcecao_QuandoQuantidadeZeroOuNegativa()
        {
            var useCase = new AdicionarInvestimentoUseCase(_repository);
            var investimento = CriarInvestimento(quantidade: 0m);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(investimento));
        }

        [TestMethod]
        public async Task AdicionarInvestimentoUseCase_DeveLancarExcecao_QuandoCotacaoNegativa()
        {
            var useCase = new AdicionarInvestimentoUseCase(_repository);
            var investimento = CriarInvestimento(cotacao: -1m);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(investimento));
        }

        [TestMethod]
        public async Task AdicionarInvestimentoUseCase_DeveAdicionar_QuandoCotacaoZero()
        {
            var useCase = new AdicionarInvestimentoUseCase(_repository);
            var investimento = CriarInvestimento(cotacao: 0m);

            await useCase.ExecuteAsync(investimento);

            Assert.AreNotEqual(0, investimento.Id);
        }

        [TestMethod]
        public async Task ObterPorIdInvestimentoUseCase_DeveRetornarInvestimentoExistente()
        {
            var investimento = CriarInvestimento("Ação XPTO");
            await _repository.AddAsync(investimento);
            var useCase = new ObterPorIdInvestimentoUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(investimento.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Ação XPTO", resultado!.Nome);
        }

        [TestMethod]
        public async Task ObterPorIdInvestimentoUseCase_DeveRetornarNulo_QuandoNaoExiste()
        {
            var useCase = new ObterPorIdInvestimentoUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task ObterTodosInvestimentoUseCase_DeveRetornarTodosInvestimentos()
        {
            await _repository.AddAsync(CriarInvestimento("Investimento 1"));
            await _repository.AddAsync(CriarInvestimento("Investimento 2"));
            var useCase = new ObterTodosInvestimentoUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarInvestimentoUseCase_DeveAtualizarInvestimento()
        {
            var investimento = CriarInvestimento("Nome Antigo");
            await _repository.AddAsync(investimento);
            var useCase = new AtualizarInvestimentoUseCase(_repository);

            investimento.Nome = "Nome Novo";
            investimento.Quantidade = 20m;
            await useCase.ExecuteAsync(investimento);

            var resultado = await _repository.GetByIdAsync(investimento.Id);
            Assert.AreEqual("Nome Novo", resultado!.Nome);
            Assert.AreEqual(20m, resultado.Quantidade);
        }

        [TestMethod]
        public async Task ExcluirInvestimentoUseCase_DeveRemoverInvestimento()
        {
            var investimento = CriarInvestimento();
            await _repository.AddAsync(investimento);
            var useCase = new ExcluirInvestimentoUseCase(_repository);

            await useCase.ExecuteAsync(investimento.Id);

            Assert.IsNull(await _repository.GetByIdAsync(investimento.Id));
        }

        [TestMethod]
        public async Task InativarInvestimentoUseCase_DeveInativarSemRemover()
        {
            var investimento = CriarInvestimento();
            await _repository.AddAsync(investimento);
            var useCase = new InativarInvestimentoUseCase(_repository);

            await useCase.ExecuteAsync(investimento.Id);

            Assert.AreEqual(1, await _context.Investimentos.CountAsync());
            var resultado = await _repository.GetByIdAsync(investimento.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }
    }
}
