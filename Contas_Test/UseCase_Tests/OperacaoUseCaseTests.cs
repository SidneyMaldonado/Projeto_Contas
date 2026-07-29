using Contas_Core.UseCase.Operacao;
using Contas_Db.Model;
using Contas_Db.Repository;
using Contas_Db.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.UseCase_Tests
{
    [TestClass]
    public sealed class OperacaoUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IRepository<Operacao> _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new Repository<Operacao>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Operacao CriarOperacao(int quantidade = 10, decimal valor = 100m, bool compra = true) => new()
        {
            IdInvestimento = 1,
            Compra = compra,
            DataOperacao = DateTime.Today,
            Quantidade = quantidade,
            ValorOperacao = valor,
            Ativo = true
        };

        [TestMethod]
        public async Task AdicionarOperacaoUseCase_DeveAdicionarOperacao()
        {
            var useCase = new AdicionarOperacaoUseCase(_repository);
            var operacao = CriarOperacao();

            await useCase.ExecuteAsync(operacao);

            Assert.AreEqual(1, await _context.Operacoes.CountAsync());
            Assert.AreNotEqual(0, operacao.Id);
        }

        [TestMethod]
        public async Task AdicionarOperacaoUseCase_DeveLancarExcecao_QuandoQuantidadeZeroOuNegativa()
        {
            var useCase = new AdicionarOperacaoUseCase(_repository);
            var operacao = CriarOperacao(quantidade: 0);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(operacao));
        }

        [TestMethod]
        public async Task AdicionarOperacaoUseCase_DeveLancarExcecao_QuandoValorOperacaoZeroOuNegativo()
        {
            var useCase = new AdicionarOperacaoUseCase(_repository);
            var operacao = CriarOperacao(valor: 0m);

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(operacao));
        }

        [TestMethod]
        public async Task AdicionarOperacaoUseCase_DeveLancarExcecao_QuandoDataOperacaoNaoInformada()
        {
            var useCase = new AdicionarOperacaoUseCase(_repository);
            var operacao = CriarOperacao();
            operacao.DataOperacao = default;

            await Assert.ThrowsExactlyAsync<ArgumentException>(() => useCase.ExecuteAsync(operacao));
        }

        [TestMethod]
        public async Task ObterPorIdOperacaoUseCase_DeveRetornarOperacaoExistente()
        {
            var operacao = CriarOperacao(quantidade: 25);
            await _repository.AddAsync(operacao);
            var useCase = new ObterPorIdOperacaoUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(operacao.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(25, resultado!.Quantidade);
        }

        [TestMethod]
        public async Task ObterTodosOperacaoUseCase_DeveRetornarTodasOperacoes()
        {
            await _repository.AddAsync(CriarOperacao());
            await _repository.AddAsync(CriarOperacao(compra: false));
            var useCase = new ObterTodosOperacaoUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarOperacaoUseCase_DeveAtualizarOperacao()
        {
            var operacao = CriarOperacao();
            await _repository.AddAsync(operacao);
            var useCase = new AtualizarOperacaoUseCase(_repository);

            operacao.ValorOperacao = 500m;
            await useCase.ExecuteAsync(operacao);

            var resultado = await _repository.GetByIdAsync(operacao.Id);
            Assert.AreEqual(500m, resultado!.ValorOperacao);
        }

        [TestMethod]
        public async Task ExcluirOperacaoUseCase_DeveRemoverOperacao()
        {
            var operacao = CriarOperacao();
            await _repository.AddAsync(operacao);
            var useCase = new ExcluirOperacaoUseCase(_repository);

            await useCase.ExecuteAsync(operacao.Id);

            Assert.IsNull(await _repository.GetByIdAsync(operacao.Id));
        }

        [TestMethod]
        public async Task InativarOperacaoUseCase_DeveInativarSemRemover()
        {
            var operacao = CriarOperacao();
            await _repository.AddAsync(operacao);
            var useCase = new InativarOperacaoUseCase(_repository);

            await useCase.ExecuteAsync(operacao.Id);

            Assert.AreEqual(1, await _context.Operacoes.CountAsync());
            var resultado = await _repository.GetByIdAsync(operacao.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }
    }
}
