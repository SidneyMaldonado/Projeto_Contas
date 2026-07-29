using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.Repository_Tests
{
    [TestClass]
    public sealed class OperacaoRepositoryTests
    {
        private ContasDbContext _context = null!;
        private Repository<Operacao> _repository = null!;

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

        private static Operacao CriarOperacao(int quantidade = 10, decimal valor = 100m) => new()
        {
            IdInvestimento = 1,
            Compra = true,
            DataOperacao = DateTime.Today,
            Quantidade = quantidade,
            ValorOperacao = valor,
            Ativo = true
        };

        [TestMethod]
        public async Task AddAsync_DeveAdicionarOperacao()
        {
            var operacao = CriarOperacao();

            await _repository.AddAsync(operacao);

            Assert.AreEqual(1, await _context.Operacoes.CountAsync());
            Assert.AreNotEqual(0, operacao.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarOperacaoExistente()
        {
            var operacao = CriarOperacao(quantidade: 15);
            await _repository.AddAsync(operacao);

            var resultado = await _repository.GetByIdAsync(operacao.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual(15, resultado!.Quantidade);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodasOperacoes()
        {
            await _repository.AddAsync(CriarOperacao());
            await _repository.AddAsync(CriarOperacao(valor: 200m));

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarOperacao()
        {
            var operacao = CriarOperacao();
            await _repository.AddAsync(operacao);

            operacao.ValorOperacao = 999m;
            operacao.Ativo = false;
            await _repository.UpdateAsync(operacao);

            var resultado = await _repository.GetByIdAsync(operacao.Id);
            Assert.AreEqual(999m, resultado!.ValorOperacao);
            Assert.IsFalse(resultado.Ativo);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverOperacao()
        {
            var operacao = CriarOperacao();
            await _repository.AddAsync(operacao);

            await _repository.DeleteAsync(operacao.Id);

            var resultado = await _repository.GetByIdAsync(operacao.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Operacoes.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarOperacaoSemRemover()
        {
            var operacao = CriarOperacao();
            await _repository.AddAsync(operacao);

            await _repository.SoftDeleteAsync(operacao.Id);

            Assert.AreEqual(1, await _context.Operacoes.CountAsync());
            var resultado = await _repository.GetByIdAsync(operacao.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Operacoes.CountAsync());
        }
    }
}
