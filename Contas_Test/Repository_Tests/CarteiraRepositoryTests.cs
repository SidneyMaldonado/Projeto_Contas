using Contas_Db.Model;
using Contas_Db.Repository;
using Contas_Db.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.Repository_Tests
{
    [TestClass]
    public sealed class CarteiraRepositoryTests
    {
        private ContasDbContext _context = null!;
        private ICarteiraRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new CarteiraRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        [TestMethod]
        public async Task AddAsync_DeveAdicionarCarteira()
        {
            var carteira = new Carteira { IdUsuario = 1, Nome = "Carteira Ações", Ativo = true };

            await _repository.AddAsync(carteira);

            Assert.AreEqual(1, await _context.Carteiras.CountAsync());
            Assert.AreNotEqual(0, carteira.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarCarteiraExistente()
        {
            var carteira = new Carteira { IdUsuario = 1, Nome = "Carteira Renda Fixa", Ativo = true };
            await _repository.AddAsync(carteira);

            var resultado = await _repository.GetByIdAsync(carteira.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Carteira Renda Fixa", resultado!.Nome);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodasCarteiras()
        {
            await _repository.AddAsync(new Carteira { IdUsuario = 1, Nome = "Carteira 1", Ativo = true });
            await _repository.AddAsync(new Carteira { IdUsuario = 1, Nome = "Carteira 2", Ativo = true });

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarCarteira()
        {
            var carteira = new Carteira { IdUsuario = 1, Nome = "Carteira Original", Ativo = true };
            await _repository.AddAsync(carteira);

            carteira.Nome = "Carteira Atualizada";
            carteira.Ativo = false;
            await _repository.UpdateAsync(carteira);

            var resultado = await _repository.GetByIdAsync(carteira.Id);
            Assert.AreEqual("Carteira Atualizada", resultado!.Nome);
            Assert.IsFalse(resultado.Ativo);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverCarteira()
        {
            var carteira = new Carteira { IdUsuario = 1, Nome = "Carteira Descartável", Ativo = true };
            await _repository.AddAsync(carteira);

            await _repository.DeleteAsync(carteira.Id);

            var resultado = await _repository.GetByIdAsync(carteira.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Carteiras.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarCarteiraSemRemover()
        {
            var carteira = new Carteira { IdUsuario = 1, Nome = "Carteira Inativável", Ativo = true };
            await _repository.AddAsync(carteira);

            await _repository.SoftDeleteAsync(carteira.Id);

            Assert.AreEqual(1, await _context.Carteiras.CountAsync());
            var resultado = await _repository.GetByIdAsync(carteira.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Carteiras.CountAsync());
        }
    }
}
