using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.Repository_Tests
{
    [TestClass]
    public sealed class InvestimentoRepositoryTests
    {
        private ContasDbContext _context = null!;
        private InvestimentoRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new InvestimentoRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Investimento CriarInvestimento(string nome = "Tesouro IPCA", int idCarteira = 1) => new()
        {
            IdCarteira = idCarteira,
            Nome = nome,
            Quantidade = 5m,
            Cotacao = 250m,
            Ativo = true
        };

        [TestMethod]
        public async Task AddAsync_DeveAdicionarInvestimento()
        {
            var investimento = CriarInvestimento();

            await _repository.AddAsync(investimento);

            Assert.AreEqual(1, await _context.Investimentos.CountAsync());
            Assert.AreNotEqual(0, investimento.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarInvestimentoExistente()
        {
            var investimento = CriarInvestimento("CDB Banco X");
            await _repository.AddAsync(investimento);

            var resultado = await _repository.GetByIdAsync(investimento.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("CDB Banco X", resultado!.Nome);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodosInvestimentos()
        {
            await _repository.AddAsync(CriarInvestimento("Investimento 1"));
            await _repository.AddAsync(CriarInvestimento("Investimento 2"));

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarInvestimento()
        {
            var investimento = CriarInvestimento("Nome Antigo");
            await _repository.AddAsync(investimento);

            investimento.Nome = "Nome Atualizado";
            investimento.Ativo = false;
            await _repository.UpdateAsync(investimento);

            var resultado = await _repository.GetByIdAsync(investimento.Id);
            Assert.AreEqual("Nome Atualizado", resultado!.Nome);
            Assert.IsFalse(resultado.Ativo);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverInvestimento()
        {
            var investimento = CriarInvestimento();
            await _repository.AddAsync(investimento);

            await _repository.DeleteAsync(investimento.Id);

            var resultado = await _repository.GetByIdAsync(investimento.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Investimentos.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarInvestimentoSemRemover()
        {
            var investimento = CriarInvestimento();
            await _repository.AddAsync(investimento);

            await _repository.SoftDeleteAsync(investimento.Id);

            Assert.AreEqual(1, await _context.Investimentos.CountAsync());
            var resultado = await _repository.GetByIdAsync(investimento.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Investimentos.CountAsync());
        }
    }
}
