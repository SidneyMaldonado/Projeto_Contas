using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.Repository_Tests
{
    [TestClass]
    public sealed class HistoricoRepositoryTests
    {
        private ContasDbContext _context = null!;
        private HistoricoRepository _repository = null!;

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

        private static Historico CriarHistorico(string nome = "Ação XYZ") => new()
        {
            IdInvestimento = 1,
            NomeInvestimento = nome,
            Quantidade = 10m,
            Cotacao = 25.50m,
            Observacao = "Compra inicial",
            DataHistorico = new DateTime(2026, 1, 10),
            Ativo = true
        };

        [TestMethod]
        public async Task AddAsync_DeveAdicionarHistorico()
        {
            var historico = CriarHistorico();

            await _repository.AddAsync(historico);

            Assert.AreEqual(1, await _context.Historicos.CountAsync());
            Assert.AreNotEqual(0, historico.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarHistoricoExistente()
        {
            var historico = CriarHistorico("Tesouro IPCA");
            await _repository.AddAsync(historico);

            var resultado = await _repository.GetByIdAsync(historico.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Tesouro IPCA", resultado!.NomeInvestimento);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodosHistoricos()
        {
            await _repository.AddAsync(CriarHistorico("Ativo A"));
            await _repository.AddAsync(CriarHistorico("Ativo B"));

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarHistorico()
        {
            var historico = CriarHistorico();
            await _repository.AddAsync(historico);

            historico.Cotacao = 40m;
            historico.Ativo = false;
            await _repository.UpdateAsync(historico);

            var resultado = await _repository.GetByIdAsync(historico.Id);
            Assert.AreEqual(40m, resultado!.Cotacao);
            Assert.IsFalse(resultado.Ativo);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverHistorico()
        {
            var historico = CriarHistorico();
            await _repository.AddAsync(historico);

            await _repository.DeleteAsync(historico.Id);

            var resultado = await _repository.GetByIdAsync(historico.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Historicos.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarHistoricoSemRemover()
        {
            var historico = CriarHistorico();
            await _repository.AddAsync(historico);

            await _repository.SoftDeleteAsync(historico.Id);

            Assert.AreEqual(1, await _context.Historicos.CountAsync());
            var resultado = await _repository.GetByIdAsync(historico.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Historicos.CountAsync());
        }
    }
}
