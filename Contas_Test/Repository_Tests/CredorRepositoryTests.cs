using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.Repository_Tests
{
    [TestClass]
    public sealed class CredorRepositoryTests
    {
        private ContasDbContext _context = null!;
        private Repository<Credor> _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new Repository<Credor>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        [TestMethod]
        public async Task AddAsync_DeveAdicionarCredor()
        {
            var credor = new Credor { Nome = "Banco X", Ativo = true };

            await _repository.AddAsync(credor);

            Assert.AreEqual(1, await _context.Credores.CountAsync());
            Assert.AreNotEqual(0, credor.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarCredorExistente()
        {
            var credor = new Credor { Nome = "Banco Y", Observacoes = "Cartão de crédito", Ativo = true };
            await _repository.AddAsync(credor);

            var resultado = await _repository.GetByIdAsync(credor.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Banco Y", resultado!.Nome);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodosCredores()
        {
            await _repository.AddAsync(new Credor { Nome = "Banco A", Ativo = true });
            await _repository.AddAsync(new Credor { Nome = "Banco B", Ativo = true });

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarCredor()
        {
            var credor = new Credor { Nome = "Banco C", Ativo = true };
            await _repository.AddAsync(credor);

            credor.Nome = "Banco C Atualizado";
            credor.Ativo = false;
            await _repository.UpdateAsync(credor);

            var resultado = await _repository.GetByIdAsync(credor.Id);
            Assert.AreEqual("Banco C Atualizado", resultado!.Nome);
            Assert.IsFalse(resultado.Ativo);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverCredor()
        {
            var credor = new Credor { Nome = "Banco D", Ativo = true };
            await _repository.AddAsync(credor);

            await _repository.DeleteAsync(credor.Id);

            var resultado = await _repository.GetByIdAsync(credor.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Credores.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarCredorSemRemover()
        {
            var credor = new Credor { Nome = "Banco E", Ativo = true };
            await _repository.AddAsync(credor);

            await _repository.SoftDeleteAsync(credor.Id);

            Assert.AreEqual(1, await _context.Credores.CountAsync());
            var resultado = await _repository.GetByIdAsync(credor.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Credores.CountAsync());
        }
    }
}
