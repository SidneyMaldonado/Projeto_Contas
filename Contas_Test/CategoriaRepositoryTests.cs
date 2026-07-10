using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test
{
    [TestClass]
    public sealed class CategoriaRepositoryTests
    {
        private ContasDbContext _context = null!;
        private Repository<Categoria> _repository = null!;

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

        [TestMethod]
        public async Task AddAsync_DeveAdicionarCategoria()
        {
            var categoria = new Categoria { Nome = "Alimentação", Ativo = true };

            await _repository.AddAsync(categoria);

            Assert.AreEqual(1, await _context.Categorias.CountAsync());
            Assert.AreNotEqual(0, categoria.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarCategoriaExistente()
        {
            var categoria = new Categoria { Nome = "Transporte", Ativo = true };
            await _repository.AddAsync(categoria);

            var resultado = await _repository.GetByIdAsync(categoria.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("Transporte", resultado!.Nome);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodasCategorias()
        {
            await _repository.AddAsync(new Categoria { Nome = "Lazer", Ativo = true });
            await _repository.AddAsync(new Categoria { Nome = "Saúde", Ativo = true });

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarCategoria()
        {
            var categoria = new Categoria { Nome = "Educação", Ativo = true };
            await _repository.AddAsync(categoria);

            categoria.Nome = "Educação Atualizada";
            categoria.Ativo = false;
            await _repository.UpdateAsync(categoria);

            var resultado = await _repository.GetByIdAsync(categoria.Id);
            Assert.AreEqual("Educação Atualizada", resultado!.Nome);
            Assert.IsFalse(resultado.Ativo);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverCategoria()
        {
            var categoria = new Categoria { Nome = "Mercado", Ativo = true };
            await _repository.AddAsync(categoria);

            await _repository.DeleteAsync(categoria.Id);

            var resultado = await _repository.GetByIdAsync(categoria.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Categorias.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarCategoriaSemRemover()
        {
            var categoria = new Categoria { Nome = "Vestuário", Ativo = true };
            await _repository.AddAsync(categoria);

            await _repository.SoftDeleteAsync(categoria.Id);

            Assert.AreEqual(1, await _context.Categorias.CountAsync());
            var resultado = await _repository.GetByIdAsync(categoria.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Categorias.CountAsync());
        }
    }
}
