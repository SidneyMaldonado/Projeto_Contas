using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test.Repository_Tests
{
    [TestClass]
    public sealed class UsuarioRepositoryTests
    {
        private ContasDbContext _context = null!;
        private Repository<Usuario> _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new Repository<Usuario>(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Usuario CriarUsuario(string nome = "João", string email = "joao@teste.com") => new()
        {
            Nome = nome,
            Email = email,
            Senha = "senha123",
            Ativo = true
        };

        [TestMethod]
        public async Task AddAsync_DeveAdicionarUsuario()
        {
            var usuario = CriarUsuario();

            await _repository.AddAsync(usuario);

            Assert.AreEqual(1, await _context.Usuarios.CountAsync());
            Assert.AreNotEqual(0, usuario.Id);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarUsuarioExistente()
        {
            var usuario = CriarUsuario();
            await _repository.AddAsync(usuario);

            var resultado = await _repository.GetByIdAsync(usuario.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("joao@teste.com", resultado!.Email);
        }

        [TestMethod]
        public async Task GetByIdAsync_DeveRetornarNuloQuandoNaoExiste()
        {
            var resultado = await _repository.GetByIdAsync(999);

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task GetAllAsync_DeveRetornarTodosUsuarios()
        {
            await _repository.AddAsync(CriarUsuario("João", "joao@teste.com"));
            await _repository.AddAsync(CriarUsuario("Maria", "maria@teste.com"));

            var resultado = await _repository.GetAllAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task UpdateAsync_DeveAtualizarUsuario()
        {
            var usuario = CriarUsuario();
            await _repository.AddAsync(usuario);

            usuario.Nome = "João Atualizado";
            usuario.Ativo = false;
            await _repository.UpdateAsync(usuario);

            var resultado = await _repository.GetByIdAsync(usuario.Id);
            Assert.AreEqual("João Atualizado", resultado!.Nome);
            Assert.IsFalse(resultado.Ativo);
        }

        [TestMethod]
        public async Task DeleteAsync_DeveRemoverUsuario()
        {
            var usuario = CriarUsuario();
            await _repository.AddAsync(usuario);

            await _repository.DeleteAsync(usuario.Id);

            var resultado = await _repository.GetByIdAsync(usuario.Id);
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task DeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.DeleteAsync(999);

            Assert.AreEqual(0, await _context.Usuarios.CountAsync());
        }

        [TestMethod]
        public async Task SoftDeleteAsync_DeveInativarUsuarioSemRemover()
        {
            var usuario = CriarUsuario();
            await _repository.AddAsync(usuario);

            await _repository.SoftDeleteAsync(usuario.Id);

            Assert.AreEqual(1, await _context.Usuarios.CountAsync());
            var resultado = await _repository.GetByIdAsync(usuario.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task SoftDeleteAsync_NaoDeveFalharQuandoIdNaoExiste()
        {
            await _repository.SoftDeleteAsync(999);

            Assert.AreEqual(0, await _context.Usuarios.CountAsync());
        }
    }
}
