using Contas_Biz.UseCase.Conta;
using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test
{
    [TestClass]
    public sealed class ContaUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IRepository<Conta> _repository = null!;

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
        public async Task AdicionarContaUseCase_DeveAdicionarConta()
        {
            var useCase = new AdicionarContaUseCase(_repository);
            var conta = CriarConta();

            await useCase.ExecuteAsync(conta);

            Assert.AreEqual(1, await _context.Contas.CountAsync());
            Assert.AreNotEqual(0, conta.Id);
        }

        [TestMethod]
        public async Task ObterPorIdContaUseCase_DeveRetornarContaExistente()
        {
            var conta = CriarConta("Conta Poupança");
            await _repository.AddAsync(conta);
            var useCase = new ObterPorIdContaUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(conta.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Conta Poupança", resultado!.Nome);
        }

        [TestMethod]
        public async Task ObterTodosContaUseCase_DeveRetornarTodasContas()
        {
            await _repository.AddAsync(CriarConta("Conta 1"));
            await _repository.AddAsync(CriarConta("Conta 2"));
            var useCase = new ObterTodosContaUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarContaUseCase_DeveAtualizarConta()
        {
            var conta = CriarConta();
            await _repository.AddAsync(conta);
            var useCase = new AtualizarContaUseCase(_repository);

            conta.Saldo = 500m;
            await useCase.ExecuteAsync(conta);

            var resultado = await _repository.GetByIdAsync(conta.Id);
            Assert.AreEqual(500m, resultado!.Saldo);
        }

        [TestMethod]
        public async Task ExcluirContaUseCase_DeveRemoverConta()
        {
            var conta = CriarConta();
            await _repository.AddAsync(conta);
            var useCase = new ExcluirContaUseCase(_repository);

            await useCase.ExecuteAsync(conta.Id);

            Assert.IsNull(await _repository.GetByIdAsync(conta.Id));
        }

        [TestMethod]
        public async Task InativarContaUseCase_DeveInativarSemRemover()
        {
            var conta = CriarConta();
            await _repository.AddAsync(conta);
            var useCase = new InativarContaUseCase(_repository);

            await useCase.ExecuteAsync(conta.Id);

            Assert.AreEqual(1, await _context.Contas.CountAsync());
            var resultado = await _repository.GetByIdAsync(conta.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }
    }
}
