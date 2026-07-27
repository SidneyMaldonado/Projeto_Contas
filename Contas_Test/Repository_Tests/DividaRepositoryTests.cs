using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.Repository_Tests
{
    [TestClass]
    public sealed class DividaRepositoryTests
    {
        private ContasDbContext _context = null!;
        private Repository<Divida> _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new Repository<Divida>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Divida CriarDivida(string nome = "Financiamento") => new()
        {
            IdUsuario = 1,
            Nome = nome,
            DiaVencimento = 10,
            DataPrimeiroVencimento = new DateTime(2026, 1, 10),
            Parcelas = 12,
            Valor = 1000m,
            Ativo = true
        };

        [TestMethod]
        public async Task AddAsync_DeveAdicionarDivida()
        {
            var divida = CriarDivida();

            await _repository.AddAsync(divida);

            Assert.AreEqual(1, await _context.Dividas.CountAsync());
            Assert.AreNotEqual(0, divida.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarDividaExistente()
        {
            var divida = CriarDivida("Empréstimo");
            await _repository.AddAsync(divida);

            var resultado = await _repository.GetByIdAsync(divida.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Empréstimo", resultado!.Nome);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodasDividas()
        {
            await _repository.AddAsync(CriarDivida("Dívida 1"));
            await _repository.AddAsync(CriarDivida("Dívida 2"));

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarDivida()
        {
            var divida = CriarDivida();
            await _repository.AddAsync(divida);

            divida.Nome = "Financiamento Atualizado";
            divida.Valor = 2000m;
            await _repository.UpdateAsync(divida);

            var resultado = await _repository.GetByIdAsync(divida.Id);
            Assert.AreEqual("Financiamento Atualizado", resultado!.Nome);
            Assert.AreEqual(2000m, resultado.Valor);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverDivida()
        {
            var divida = CriarDivida();
            await _repository.AddAsync(divida);

            await _repository.DeleteAsync(divida.Id);

            var resultado = await _repository.GetByIdAsync(divida.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Dividas.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarDividaSemRemover()
        {
            var divida = CriarDivida();
            await _repository.AddAsync(divida);

            await _repository.SoftDeleteAsync(divida.Id);

            Assert.AreEqual(1, await _context.Dividas.CountAsync());
            var resultado = await _repository.GetByIdAsync(divida.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Dividas.CountAsync());
        }
    }
}
