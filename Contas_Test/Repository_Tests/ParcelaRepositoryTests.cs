using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.Repository_Tests
{
    [TestClass]
    public sealed class ParcelaRepositoryTests
    {
        private ContasDbContext _context = null!;
        private Repository<Parcela> _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new Repository<Parcela>(_context);
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
        public async Task AddAsync_DeveAdicionarParcela()
        {
            var parcela = CriarParcela();

            await _repository.AddAsync(parcela);

            Assert.AreEqual(1, await _context.Parcelas.CountAsync());
            Assert.AreNotEqual(0, parcela.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarParcelaExistente()
        {
            var parcela = CriarParcela("Parcela 2/12");
            await _repository.AddAsync(parcela);

            var resultado = await _repository.GetByIdAsync(parcela.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Parcela 2/12", resultado!.Descricao);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodasParcelas()
        {
            await _repository.AddAsync(CriarParcela("Parcela A"));
            await _repository.AddAsync(CriarParcela("Parcela B"));

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarParcela()
        {
            var parcela = CriarParcela();
            await _repository.AddAsync(parcela);

            parcela.DataPagamento = new DateTime(2026, 1, 9);
            parcela.Valor = 150m;
            await _repository.UpdateAsync(parcela);

            var resultado = await _repository.GetByIdAsync(parcela.Id);
            Assert.AreEqual(150m, resultado!.Valor);
            Assert.IsNotNull(resultado.DataPagamento);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverParcela()
        {
            var parcela = CriarParcela();
            await _repository.AddAsync(parcela);

            await _repository.DeleteAsync(parcela.Id);

            var resultado = await _repository.GetByIdAsync(parcela.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Parcelas.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarParcelaSemRemover()
        {
            var parcela = CriarParcela();
            await _repository.AddAsync(parcela);

            await _repository.SoftDeleteAsync(parcela.Id);

            Assert.AreEqual(1, await _context.Parcelas.CountAsync());
            var resultado = await _repository.GetByIdAsync(parcela.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Parcelas.CountAsync());
        }

        [TestMethod]
        public async Task PagarAsync_DeveAtualizarDataPagamento()
        {
            var parcela = CriarParcela();
            await _repository.AddAsync(parcela);
            var parcelaRepository = new ParcelaRepository(_context);
            var dataPagamento = new DateTime(2026, 1, 8);

            await parcelaRepository.PagarAsync(parcela.Id, dataPagamento);

            var resultado = await _repository.GetByIdAsync(parcela.Id);
            Assert.IsNotNull(resultado);
            Assert.AreEqual(dataPagamento, resultado!.DataPagamento);
            Assert.IsTrue(resultado.Pago);
        }

        [TestMethod]
        public async Task PagarAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            var parcelaRepository = new ParcelaRepository(_context);

            await parcelaRepository.PagarAsync(999, new DateTime(2026, 1, 8));

            Assert.AreEqual(0, await _context.Parcelas.CountAsync());
        }

        [TestMethod]
        public async Task DesfazerPagamentoAsync_DeveLimparDataPagamento()
        {
            var parcela = CriarParcela();
            parcela.DataPagamento = new DateTime(2026, 1, 8);
            await _repository.AddAsync(parcela);
            var parcelaRepository = new ParcelaRepository(_context);

            await parcelaRepository.DesfazerPagamentoAsync(parcela.Id);

            var resultado = await _repository.GetByIdAsync(parcela.Id);
            Assert.IsNotNull(resultado);
            Assert.IsNull(resultado!.DataPagamento);
            Assert.IsFalse(resultado.Pago);
        }

        [TestMethod]
        public async Task DesfazerPagamentoAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            var parcelaRepository = new ParcelaRepository(_context);

            await parcelaRepository.DesfazerPagamentoAsync(999);

            Assert.AreEqual(0, await _context.Parcelas.CountAsync());
        }
    }
}
