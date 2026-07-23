using Contas_Core.UseCase.Parcela;
using Contas_Db.Model;
using Contas_Db.Repository;
using Contas_Db.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test
{
    [TestClass]
    public sealed class ParcelaUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IParcelaRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new ParcelaRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Parcela CriarParcela(string descricao = "Parcela 1/12") => new()
        {
            IdDivida = 1,
            IdCategoria = 1,
            IdConta = 1,
            Descricao = descricao,
            Valor = 100m,
            DataVencimento = new DateTime(2026, 1, 10),
            Ativo = true
        };

        [TestMethod]
        public async Task AdicionarParcelaUseCase_DeveAdicionarParcela()
        {
            var useCase = new AdicionarParcelaUseCase(_repository);
            var parcela = CriarParcela();

            await useCase.ExecuteAsync(parcela);

            Assert.AreEqual(1, await _context.Parcelas.CountAsync());
            Assert.AreNotEqual(0, parcela.Id);
        }

        [TestMethod]
        public async Task AdicionarParcelaUseCase_DeveLancarExcecao_QuandoValorNegativoOuZero()
        {
            var useCase = new AdicionarParcelaUseCase(_repository);
            var parcela = CriarParcela();
            parcela.Valor = 0m;

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(parcela));
        }

        [TestMethod]
        public async Task ObterPorIdParcelaUseCase_DeveRetornarParcelaExistente()
        {
            var parcela = CriarParcela("Parcela 2/12");
            await _repository.AddAsync(parcela);
            var useCase = new ObterPorIdParcelaUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(parcela.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Parcela 2/12", resultado!.Descricao);
        }

        [TestMethod]
        public async Task ObterTodosParcelaUseCase_DeveRetornarTodasParcelas()
        {
            await _repository.AddAsync(CriarParcela("Parcela A"));
            await _repository.AddAsync(CriarParcela("Parcela B"));
            var useCase = new ObterTodosParcelaUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarParcelaUseCase_DeveAtualizarParcela()
        {
            var parcela = CriarParcela();
            await _repository.AddAsync(parcela);
            var useCase = new AtualizarParcelaUseCase(_repository);

            parcela.Valor = 150m;
            await useCase.ExecuteAsync(parcela);

            var resultado = await _repository.GetByIdAsync(parcela.Id);
            Assert.AreEqual(150m, resultado!.Valor);
        }

        [TestMethod]
        public async Task ExcluirParcelaUseCase_DeveRemoverParcela()
        {
            var parcela = CriarParcela();
            await _repository.AddAsync(parcela);
            var useCase = new ExcluirParcelaUseCase(_repository);

            await useCase.ExecuteAsync(parcela.Id);

            Assert.IsNull(await _repository.GetByIdAsync(parcela.Id));
        }

        [TestMethod]
        public async Task InativarParcelaUseCase_DeveInativarSemRemover()
        {
            var parcela = CriarParcela();
            await _repository.AddAsync(parcela);
            var useCase = new InativarParcelaUseCase(_repository);

            await useCase.ExecuteAsync(parcela.Id);

            Assert.AreEqual(1, await _context.Parcelas.CountAsync());
            var resultado = await _repository.GetByIdAsync(parcela.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task PagarParcelaUseCase_DeveAtualizarDataPagamento()
        {
            var parcela = CriarParcela();
            await _repository.AddAsync(parcela);
            var useCase = new PagarParcelaUseCase(_repository);
            var dataPagamento = new DateTime(2026, 1, 8);

            await useCase.ExecuteAsync(parcela.Id, dataPagamento);

            var resultado = await _repository.GetByIdAsync(parcela.Id);
            Assert.IsNotNull(resultado);
            Assert.AreEqual(dataPagamento, resultado!.DataPagamento);
            Assert.IsTrue(resultado.Pago);
        }

        [TestMethod]
        public async Task DesfazerPagamentoParcelaUseCase_DeveLimparDataPagamento()
        {
            var parcela = CriarParcela();
            parcela.DataPagamento = new DateTime(2026, 1, 8);
            await _repository.AddAsync(parcela);
            var useCase = new DesfazerPagamentoParcelaUseCase(_repository);

            await useCase.ExecuteAsync(parcela.Id);

            var resultado = await _repository.GetByIdAsync(parcela.Id);
            Assert.IsNotNull(resultado);
            Assert.IsNull(resultado!.DataPagamento);
            Assert.IsFalse(resultado.Pago);
        }
    }
}
