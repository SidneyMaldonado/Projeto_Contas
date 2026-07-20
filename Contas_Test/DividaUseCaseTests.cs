using Contas_Core.UseCase.Divida;
using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test
{
    [TestClass]
    public sealed class DividaUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IRepository<Divida> _repository = null!;

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
        public async Task AdicionarDividaUseCase_DeveAdicionarDivida()
        {
            var useCase = new AdicionarDividaUseCase(_repository);
            var divida = CriarDivida();

            await useCase.ExecuteAsync(divida);

            Assert.AreEqual(1, await _context.Dividas.CountAsync());
            Assert.AreNotEqual(0, divida.Id);
        }

        [TestMethod]
        public async Task ObterPorIdDividaUseCase_DeveRetornarDividaExistente()
        {
            var divida = CriarDivida("Empréstimo");
            await _repository.AddAsync(divida);
            var useCase = new ObterPorIdDividaUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(divida.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Empréstimo", resultado!.Nome);
        }

        [TestMethod]
        public async Task ObterTodosDividaUseCase_DeveRetornarTodasDividas()
        {
            await _repository.AddAsync(CriarDivida("Dívida 1"));
            await _repository.AddAsync(CriarDivida("Dívida 2"));
            var useCase = new ObterTodosDividaUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarDividaUseCase_DeveAtualizarDivida()
        {
            var divida = CriarDivida();
            await _repository.AddAsync(divida);
            var useCase = new AtualizarDividaUseCase(_repository);

            divida.Valor = 2000m;
            await useCase.ExecuteAsync(divida);

            var resultado = await _repository.GetByIdAsync(divida.Id);
            Assert.AreEqual(2000m, resultado!.Valor);
        }

        [TestMethod]
        public async Task ExcluirDividaUseCase_DeveRemoverDivida()
        {
            var divida = CriarDivida();
            await _repository.AddAsync(divida);
            var useCase = new ExcluirDividaUseCase(_repository);

            await useCase.ExecuteAsync(divida.Id);

            Assert.IsNull(await _repository.GetByIdAsync(divida.Id));
        }

        [TestMethod]
        public async Task InativarDividaUseCase_DeveInativarSemRemover()
        {
            var divida = CriarDivida();
            await _repository.AddAsync(divida);
            var useCase = new InativarDividaUseCase(_repository);

            await useCase.ExecuteAsync(divida.Id);

            Assert.AreEqual(1, await _context.Dividas.CountAsync());
            var resultado = await _repository.GetByIdAsync(divida.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }
    }
}
