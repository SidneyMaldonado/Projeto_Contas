using Contas_Core.UseCase.Credor;
using Contas_Db.Model;
using Contas_Db.Repository;
using Contas_Db.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.UseCase_Tests
{
    [TestClass]
    public sealed class CredorUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IRepository<Credor> _repository = null!;

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

        private static Credor CriarCredor(string nome = "Banco X") => new()
        {
            Nome = nome,
            Ativo = true
        };

        [TestMethod]
        public async Task AdicionarCredorUseCase_DeveAdicionarCredor()
        {
            var useCase = new AdicionarCredorUseCase(_repository);
            var credor = CriarCredor();

            await useCase.ExecuteAsync(credor);

            Assert.AreEqual(1, await _context.Credores.CountAsync());
            Assert.AreNotEqual(0, credor.Id);
        }

        [TestMethod]
        public async Task AdicionarCredorUseCase_DeveLancarExcecao_QuandoNomeMenorQue3Caracteres()
        {
            var useCase = new AdicionarCredorUseCase(_repository);
            var credor = CriarCredor("Ba");

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(credor));
        }

        [TestMethod]
        public async Task ObterPorIdCredorUseCase_DeveRetornarCredorExistente()
        {
            var credor = CriarCredor("Banco Y");
            await _repository.AddAsync(credor);
            var useCase = new ObterPorIdCredorUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(credor.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Banco Y", resultado!.Nome);
        }

        [TestMethod]
        public async Task ObterTodosCredorUseCase_DeveRetornarTodosCredores()
        {
            await _repository.AddAsync(CriarCredor("Banco A"));
            await _repository.AddAsync(CriarCredor("Banco B"));
            var useCase = new ObterTodosCredorUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarCredorUseCase_DeveAtualizarCredor()
        {
            var credor = CriarCredor("Banco C");
            await _repository.AddAsync(credor);
            var useCase = new AtualizarCredorUseCase(_repository);

            credor.Nome = "Banco C Atualizado";
            await useCase.ExecuteAsync(credor);

            var resultado = await _repository.GetByIdAsync(credor.Id);
            Assert.AreEqual("Banco C Atualizado", resultado!.Nome);
        }

        [TestMethod]
        public async Task ExcluirCredorUseCase_DeveRemoverCredor()
        {
            var credor = CriarCredor("Banco D");
            await _repository.AddAsync(credor);
            var useCase = new ExcluirCredorUseCase(_repository);

            await useCase.ExecuteAsync(credor.Id);

            Assert.IsNull(await _repository.GetByIdAsync(credor.Id));
        }

        [TestMethod]
        public async Task InativarCredorUseCase_DeveInativarSemRemover()
        {
            var credor = CriarCredor("Banco E");
            await _repository.AddAsync(credor);
            var useCase = new InativarCredorUseCase(_repository);

            await useCase.ExecuteAsync(credor.Id);

            Assert.AreEqual(1, await _context.Credores.CountAsync());
            var resultado = await _repository.GetByIdAsync(credor.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }
    }
}
