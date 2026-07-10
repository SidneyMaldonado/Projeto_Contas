using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test
{
    [TestClass]
    public sealed class ContaRepositoryTests
    {
        private ContasDbContext _context = null!;
        private Repository<Conta> _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new Repository<Conta>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Conta CriarConta(string nome = "Conta Corrente") => new()
        {
            IdUsuario = 1,
            Nome = nome,
            Saldo = 100.50m,
            Ativo = true
        };

        [TestMethod]
        public async Task AddAsync_DeveAdicionarConta()
        {
            var conta = CriarConta();

            await _repository.AddAsync(conta);

            Assert.AreEqual(1, await _context.Contas.CountAsync());
            Assert.AreNotEqual(0, conta.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarContaExistente()
        {
            var conta = CriarConta("Conta Poupança");
            await _repository.AddAsync(conta);

            var resultado = await _repository.GetByIdAsync(conta.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Conta Poupança", resultado!.Nome);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodasContas()
        {
            await _repository.AddAsync(CriarConta("Conta 1"));
            await _repository.AddAsync(CriarConta("Conta 2"));

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarConta()
        {
            var conta = CriarConta();
            await _repository.AddAsync(conta);

            conta.Nome = "Conta Atualizada";
            conta.Saldo = 500m;
            await _repository.UpdateAsync(conta);

            var resultado = await _repository.GetByIdAsync(conta.Id);
            Assert.AreEqual("Conta Atualizada", resultado!.Nome);
            Assert.AreEqual(500m, resultado.Saldo);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverConta()
        {
            var conta = CriarConta();
            await _repository.AddAsync(conta);

            await _repository.DeleteAsync(conta.Id);

            var resultado = await _repository.GetByIdAsync(conta.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Contas.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarContaSemRemover()
        {
            var conta = CriarConta();
            await _repository.AddAsync(conta);

            await _repository.SoftDeleteAsync(conta.Id);

            Assert.AreEqual(1, await _context.Contas.CountAsync());
            var resultado = await _repository.GetByIdAsync(conta.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Contas.CountAsync());
        }
    }
}
