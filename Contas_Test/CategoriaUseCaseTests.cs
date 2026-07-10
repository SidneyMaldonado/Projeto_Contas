using Contas_Biz.UseCase.Categoria;
using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test
{
    [TestClass]
    public sealed class CategoriaUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IRepository<Categoria> _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new Repository<Categoria>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Categoria CriarCategoria(string nome = "Lazer") => new()
        {
            Nome = nome,
            Ativo = true
        };

        [TestMethod]
        public async Task AdicionarCategoriaUseCase_DeveAdicionarCategoria()
        {
            var useCase = new AdicionarCategoriaUseCase(_repository);
            var categoria = CriarCategoria();

            await useCase.ExecuteAsync(categoria);

            Assert.AreEqual(1, await _context.Categorias.CountAsync());
            Assert.AreNotEqual(0, categoria.Id);
        }

        [TestMethod]
        public async Task ObterPorIdCategoriaUseCase_DeveRetornarCategoriaExistente()
        {
            var categoria = CriarCategoria("Transporte");
            await _repository.AddAsync(categoria);
            var useCase = new ObterPorIdCategoriaUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(categoria.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Transporte", resultado!.Nome);
        }

        [TestMethod]
        public async Task ObterTodosCategoriaUseCase_DeveRetornarTodasCategorias()
        {
            await _repository.AddAsync(CriarCategoria("Lazer"));
            await _repository.AddAsync(CriarCategoria("Saúde"));
            var useCase = new ObterTodosCategoriaUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarCategoriaUseCase_DeveAtualizarCategoria()
        {
            var categoria = CriarCategoria("Educação");
            await _repository.AddAsync(categoria);
            var useCase = new AtualizarCategoriaUseCase(_repository);

            categoria.Nome = "Educação Atualizada";
            await useCase.ExecuteAsync(categoria);

            var resultado = await _repository.GetByIdAsync(categoria.Id);
            Assert.AreEqual("Educação Atualizada", resultado!.Nome);
        }

        [TestMethod]
        public async Task ExcluirCategoriaUseCase_DeveRemoverCategoria()
        {
            var categoria = CriarCategoria("Mercado");
            await _repository.AddAsync(categoria);
            var useCase = new ExcluirCategoriaUseCase(_repository);

            await useCase.ExecuteAsync(categoria.Id);

            Assert.IsNull(await _repository.GetByIdAsync(categoria.Id));
        }

        [TestMethod]
        public async Task InativarCategoriaUseCase_DeveInativarSemRemover()
        {
            var categoria = CriarCategoria("Vestuário");
            await _repository.AddAsync(categoria);
            var useCase = new InativarCategoriaUseCase(_repository);

            await useCase.ExecuteAsync(categoria.Id);

            Assert.AreEqual(1, await _context.Categorias.CountAsync());
            var resultado = await _repository.GetByIdAsync(categoria.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }
    }
}
