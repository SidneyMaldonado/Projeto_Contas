using Contas_Core.UseCase.Carteira;
using Contas_Db.Model;
using Contas_Db.Repository;
using Contas_Db.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.UseCase_Tests
{
    [TestClass]
    public sealed class CarteiraUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IRepository<Carteira> _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new Repository<Carteira>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Carteira CriarCarteira(string nome = "Carteira Renda Fixa") => new()
        {
            IdUsuario = 1,
            Nome = nome,
            Ativo = true
        };

        [TestMethod]
        public async Task AdicionarCarteiraUseCase_DeveAdicionarCarteira()
        {
            var useCase = new AdicionarCarteiraUseCase(_repository);
            var carteira = CriarCarteira();

            await useCase.ExecuteAsync(carteira);

            Assert.AreEqual(1, await _context.Carteiras.CountAsync());
            Assert.AreNotEqual(0, carteira.Id);
        }

        [TestMethod]
        public async Task AdicionarCarteiraUseCase_DeveLancarExcecao_QuandoNomeVazio()
        {
            var useCase = new AdicionarCarteiraUseCase(_repository);
            var carteira = CriarCarteira("");

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(carteira));
        }

        [TestMethod]
        public async Task AdicionarCarteiraUseCase_DeveLancarExcecao_QuandoNomeMenorQue3Caracteres()
        {
            var useCase = new AdicionarCarteiraUseCase(_repository);
            var carteira = CriarCarteira("Ca");

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(carteira));
        }

        [TestMethod]
        public async Task AdicionarCarteiraUseCase_DeveLancarExcecao_QuandoNomeMaiorQue50Caracteres()
        {
            var useCase = new AdicionarCarteiraUseCase(_repository);
            var carteira = CriarCarteira(new string('A', 51));

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(carteira));
        }

        [TestMethod]
        public async Task AdicionarCarteiraUseCase_DeveAdicionar_QuandoNomeTem50Caracteres()
        {
            var useCase = new AdicionarCarteiraUseCase(_repository);
            var carteira = CriarCarteira(new string('A', 50));

            await useCase.ExecuteAsync(carteira);

            Assert.AreNotEqual(0, carteira.Id);
        }

        [TestMethod]
        public async Task ObterPorIdCarteiraUseCase_DeveRetornarCarteiraExistente()
        {
            var carteira = CriarCarteira("Carteira Cripto");
            await _repository.AddAsync(carteira);
            var useCase = new ObterPorIdCarteiraUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(carteira.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Carteira Cripto", resultado!.Nome);
        }

        [TestMethod]
        public async Task ObterPorIdCarteiraUseCase_DeveRetornarNulo_QuandoNaoExiste()
        {
            var useCase = new ObterPorIdCarteiraUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task ObterTodosCarteiraUseCase_DeveRetornarTodasCarteiras()
        {
            await _repository.AddAsync(CriarCarteira("Carteira 1"));
            await _repository.AddAsync(CriarCarteira("Carteira 2"));
            var useCase = new ObterTodosCarteiraUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarCarteiraUseCase_DeveAtualizarCarteira()
        {
            var carteira = CriarCarteira();
            await _repository.AddAsync(carteira);
            var useCase = new AtualizarCarteiraUseCase(_repository);

            carteira.Nome = "Carteira Renomeada";
            await useCase.ExecuteAsync(carteira);

            var resultado = await _repository.GetByIdAsync(carteira.Id);
            Assert.AreEqual("Carteira Renomeada", resultado!.Nome);
        }

        [TestMethod]
        public async Task ExcluirCarteiraUseCase_DeveRemoverCarteira()
        {
            var carteira = CriarCarteira();
            await _repository.AddAsync(carteira);
            var useCase = new ExcluirCarteiraUseCase(_repository);

            await useCase.ExecuteAsync(carteira.Id);

            Assert.IsNull(await _repository.GetByIdAsync(carteira.Id));
        }

        [TestMethod]
        public async Task InativarCarteiraUseCase_DeveInativarSemRemover()
        {
            var carteira = CriarCarteira();
            await _repository.AddAsync(carteira);
            var useCase = new InativarCarteiraUseCase(_repository);

            await useCase.ExecuteAsync(carteira.Id);

            Assert.AreEqual(1, await _context.Carteiras.CountAsync());
            var resultado = await _repository.GetByIdAsync(carteira.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }
    }
}
