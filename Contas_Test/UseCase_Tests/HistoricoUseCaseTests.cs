using Contas_Core.UseCase.Historico;
using Contas_Db.Model;
using Contas_Db.Repository;
using Contas_Db.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.UseCase_Tests
{
    [TestClass]
    public sealed class HistoricoUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IHistoricoRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new HistoricoRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Historico CriarHistorico(string nome = "Ação XYZ", decimal quantidade = 10m, decimal cotacao = 25.50m, string observacao = "Compra inicial") => new()
        {
            IdInvestimento = 1,
            NomeInvestimento = nome,
            Quantidade = quantidade,
            Cotacao = cotacao,
            Observacao = observacao,
            DataHistorico = new DateTime(2026, 1, 10),
            Ativo = true
        };

        [TestMethod]
        public async Task AdicionarHistoricoUseCase_DeveAdicionarHistorico()
        {
            var useCase = new AdicionarHistoricoUseCase(_repository);
            var historico = CriarHistorico();

            await useCase.ExecuteAsync(historico);

            Assert.AreEqual(1, await _context.Historicos.CountAsync());
            Assert.AreNotEqual(0, historico.Id);
        }

        [TestMethod]
        public async Task AdicionarHistoricoUseCase_DevePreencherDataHistorico_QuandoNaoInformada()
        {
            var useCase = new AdicionarHistoricoUseCase(_repository);
            var historico = CriarHistorico();
            historico.DataHistorico = default;

            var antes = DateTime.UtcNow;
            await useCase.ExecuteAsync(historico);
            var depois = DateTime.UtcNow;

            Assert.IsTrue(historico.DataHistorico >= antes && historico.DataHistorico <= depois);
        }

        [TestMethod]
        public async Task AdicionarHistoricoUseCase_DeveLancarExcecao_QuandoNomeInvestimentoVazio()
        {
            var useCase = new AdicionarHistoricoUseCase(_repository);
            var historico = CriarHistorico(nome: "");

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(historico));
        }

        [TestMethod]
        public async Task AdicionarHistoricoUseCase_DeveLancarExcecao_QuandoQuantidadeNegativa()
        {
            var useCase = new AdicionarHistoricoUseCase(_repository);
            var historico = CriarHistorico(quantidade: -1m);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(historico));
        }

        [TestMethod]
        public async Task AdicionarHistoricoUseCase_DeveLancarExcecao_QuandoCotacaoNegativa()
        {
            var useCase = new AdicionarHistoricoUseCase(_repository);
            var historico = CriarHistorico(cotacao: -1m);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(historico));
        }

        [TestMethod]
        public async Task AdicionarHistoricoUseCase_DeveLancarExcecao_QuandoObservacaoVazia()
        {
            var useCase = new AdicionarHistoricoUseCase(_repository);
            var historico = CriarHistorico(observacao: "");

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(historico));
        }

        [TestMethod]
        public async Task ObterPorIdHistoricoUseCase_DeveRetornarHistoricoExistente()
        {
            var historico = CriarHistorico("Tesouro Selic");
            await _repository.AddAsync(historico);
            var useCase = new ObterPorIdHistoricoUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(historico.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Tesouro Selic", resultado!.NomeInvestimento);
        }

        [TestMethod]
        public async Task ObterTodosHistoricoUseCase_DeveRetornarTodosHistoricos()
        {
            await _repository.AddAsync(CriarHistorico("Ativo A"));
            await _repository.AddAsync(CriarHistorico("Ativo B"));
            var useCase = new ObterTodosHistoricoUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarHistoricoUseCase_DeveAtualizarHistorico()
        {
            var historico = CriarHistorico();
            await _repository.AddAsync(historico);
            var useCase = new AtualizarHistoricoUseCase(_repository);

            historico.Cotacao = 30.75m;
            await useCase.ExecuteAsync(historico);

            var resultado = await _repository.GetByIdAsync(historico.Id);
            Assert.AreEqual(30.75m, resultado!.Cotacao);
        }

        [TestMethod]
        public async Task ExcluirHistoricoUseCase_DeveRemoverHistorico()
        {
            var historico = CriarHistorico();
            await _repository.AddAsync(historico);
            var useCase = new ExcluirHistoricoUseCase(_repository);

            await useCase.ExecuteAsync(historico.Id);

            Assert.IsNull(await _repository.GetByIdAsync(historico.Id));
        }

        [TestMethod]
        public async Task InativarHistoricoUseCase_DeveInativarSemRemover()
        {
            var historico = CriarHistorico();
            await _repository.AddAsync(historico);
            var useCase = new InativarHistoricoUseCase(_repository);

            await useCase.ExecuteAsync(historico.Id);

            Assert.AreEqual(1, await _context.Historicos.CountAsync());
            var resultado = await _repository.GetByIdAsync(historico.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }
    }
}
