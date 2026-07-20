using Contas_Core.Security;
using Contas_Core.UseCase.Usuario;
using Contas_Db.Model;
using Contas_Db.Repository;
using Microsoft.EntityFrameworkCore;

namespace Contas_Test
{
    [TestClass]
    public sealed class UsuarioUseCaseTests
    {
        private ContasDbContext _context = null!;
        private IUsuarioRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ContasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ContasDbContext(options);
            _repository = new UsuarioRepository(_context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context.Dispose();
        }

        private static Usuario CriarUsuario(string nome = "João", string email = "joao@teste.com", string senha = "senha123") => new()
        {
            Nome = nome,
            Email = email,
            Senha = senha,
            Ativo = true
        };

        [TestMethod]
        public async Task AdicionarUsuarioUseCase_DeveHashearSenhaAntesDePersistir()
        {
            var useCase = new AdicionarUsuarioUseCase(_repository);
            var usuario = CriarUsuario(senha: "senha123");

            await useCase.ExecuteAsync(usuario);

            var resultado = await _repository.GetByIdAsync(usuario.Id);
            Assert.IsNotNull(resultado);
            Assert.AreNotEqual("senha123", resultado!.Senha);
            Assert.IsTrue(PasswordHasher.Verify("senha123", resultado.Senha));
        }

        [TestMethod]
        public async Task ObterPorIdUsuarioUseCase_DeveRetornarUsuarioExistente()
        {
            var usuario = CriarUsuario();
            await _repository.AddAsync(usuario);
            var useCase = new ObterPorIdUsuarioUseCase(_repository);

            var resultado = await useCase.ExecuteAsync(usuario.Id);

            Assert.IsNotNull(resultado);
            Assert.AreEqual("joao@teste.com", resultado!.Email);
        }

        [TestMethod]
        public async Task ObterTodosUsuarioUseCase_DeveRetornarTodosUsuarios()
        {
            await _repository.AddAsync(CriarUsuario("João", "joao@teste.com"));
            await _repository.AddAsync(CriarUsuario("Maria", "maria@teste.com"));
            var useCase = new ObterTodosUsuarioUseCase(_repository);

            var resultado = await useCase.ExecuteAsync();

            Assert.AreEqual(2, resultado.Count());
        }

        [TestMethod]
        public async Task AtualizarUsuarioUseCase_NaoDeveAlterarHashDaSenha()
        {
            var usuario = CriarUsuario();
            var adicionar = new AdicionarUsuarioUseCase(_repository);
            await adicionar.ExecuteAsync(usuario);
            var hashOriginal = usuario.Senha;
            var useCase = new AtualizarUsuarioUseCase(_repository);

            usuario.Nome = "João Atualizado";
            await useCase.ExecuteAsync(usuario);

            var resultado = await _repository.GetByIdAsync(usuario.Id);
            Assert.AreEqual("João Atualizado", resultado!.Nome);
            Assert.AreEqual(hashOriginal, resultado.Senha);
        }

        [TestMethod]
        public async Task ExcluirUsuarioUseCase_DeveRemoverUsuario()
        {
            var usuario = CriarUsuario();
            await _repository.AddAsync(usuario);
            var useCase = new ExcluirUsuarioUseCase(_repository);

            await useCase.ExecuteAsync(usuario.Id);

            Assert.IsNull(await _repository.GetByIdAsync(usuario.Id));
        }

        [TestMethod]
        public async Task InativarUsuarioUseCase_DeveInativarSemRemover()
        {
            var usuario = CriarUsuario();
            await _repository.AddAsync(usuario);
            var useCase = new InativarUsuarioUseCase(_repository);

            await useCase.ExecuteAsync(usuario.Id);

            Assert.AreEqual(1, await _context.Usuarios.CountAsync());
            var resultado = await _repository.GetByIdAsync(usuario.Id);
            Assert.IsNotNull(resultado);
            Assert.IsFalse(resultado!.Ativo);
        }

        [TestMethod]
        public async Task LoginUsuarioUseCase_DeveRetornarUsuarioComSenhaCorreta()
        {
            var usuario = CriarUsuario(senha: "senha123");
            await new AdicionarUsuarioUseCase(_repository).ExecuteAsync(usuario);
            var useCase = new LoginUsuarioUseCase(_repository);

            var resultado = await useCase.ExecuteAsync("joao@teste.com", "senha123");

            Assert.IsNotNull(resultado);
            Assert.AreEqual(usuario.Id, resultado!.Id);
        }

        [TestMethod]
        public async Task LoginUsuarioUseCase_DeveRetornarNuloComSenhaIncorreta()
        {
            var usuario = CriarUsuario(senha: "senha123");
            await new AdicionarUsuarioUseCase(_repository).ExecuteAsync(usuario);
            var useCase = new LoginUsuarioUseCase(_repository);

            var resultado = await useCase.ExecuteAsync("joao@teste.com", "senhaErrada");

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task LoginUsuarioUseCase_DeveRetornarNuloQuandoUsuarioInativo()
        {
            var usuario = CriarUsuario(senha: "senha123");
            await new AdicionarUsuarioUseCase(_repository).ExecuteAsync(usuario);
            await _repository.SoftDeleteAsync(usuario.Id);
            var useCase = new LoginUsuarioUseCase(_repository);

            var resultado = await useCase.ExecuteAsync("joao@teste.com", "senha123");

            Assert.IsNull(resultado);
        }

        [TestMethod]
        public async Task AlterarSenhaUsuarioUseCase_DeveTrocarSenhaComSenhaAtualCorreta()
        {
            var usuario = CriarUsuario(senha: "senhaAntiga");
            await new AdicionarUsuarioUseCase(_repository).ExecuteAsync(usuario);
            var alterarSenha = new AlterarSenhaUsuarioUseCase(_repository);

            var sucesso = await alterarSenha.ExecuteAsync(usuario.Id, "senhaAntiga", "senhaNova");

            Assert.IsTrue(sucesso);
            var login = new LoginUsuarioUseCase(_repository);
            Assert.IsNotNull(await login.ExecuteAsync("joao@teste.com", "senhaNova"));
            Assert.IsNull(await login.ExecuteAsync("joao@teste.com", "senhaAntiga"));
        }

        [TestMethod]
        public async Task AlterarSenhaUsuarioUseCase_DeveFalharComSenhaAtualIncorreta()
        {
            var usuario = CriarUsuario(senha: "senhaAntiga");
            await new AdicionarUsuarioUseCase(_repository).ExecuteAsync(usuario);
            var alterarSenha = new AlterarSenhaUsuarioUseCase(_repository);

            var sucesso = await alterarSenha.ExecuteAsync(usuario.Id, "senhaErrada", "senhaNova");

            Assert.IsFalse(sucesso);
            var login = new LoginUsuarioUseCase(_repository);
            Assert.IsNotNull(await login.ExecuteAsync("joao@teste.com", "senhaAntiga"));
        }
    }
}
