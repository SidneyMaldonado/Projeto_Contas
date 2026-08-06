using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Contratos.Dto;
using Contas_Core.Security;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class UsuarioControllerTests : ApiTestBase
    {
        private const string SenhaPadrao = "Senha123";

        private Task<Usuario> SeedUsuarioAsync(
            string nome = "Fulano de Tal",
            string email = "fulano@teste.com",
            string senha = SenhaPadrao,
            bool ativo = true) =>
            SeedAsync(new Usuario
            {
                Nome = nome,
                Email = email,
                Senha = PasswordHasher.Hash(senha),
                Ativo = ativo
            });

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeUsuarios()
        {
            await SeedUsuarioAsync();

            var response = await Client.GetAsync("/api/usuarios");
            response.EnsureSuccessStatusCode();

            var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioDto>>();

            Assert.IsNotNull(usuarios);
            Assert.IsNotEmpty(usuarios);
        }

        [TestMethod]
        public async Task ObterTodos_NaoDeveExporSenha()
        {
            await SeedUsuarioAsync();

            var response = await Client.GetAsync("/api/usuarios");
            var json = await response.Content.ReadAsStringAsync();

            Assert.IsFalse(json.Contains("Senha", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarUsuario_QuandoExistir()
        {
            var usuario = await SeedUsuarioAsync(nome: "Maria Silva");

            var response = await Client.GetAsync($"/api/usuarios/{usuario.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<UsuarioDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(usuario.Id, dto!.Id);
            Assert.AreEqual("Maria Silva", dto.Nome);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/usuarios/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarUsuario_QuandoValido()
        {
            var dto = new AdicionarUsuarioDto
            {
                Nome = "Novo Usuario",
                Email = "novo@teste.com",
                Senha = SenhaPadrao
            };

            var response = await Client.PostAsJsonAsync("/api/usuarios", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criado = await response.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.IsNotNull(criado);
            Assert.AreEqual("Novo Usuario", criado!.Nome);
            Assert.AreNotEqual(0, criado.Id);
            Assert.IsTrue(criado.Ativo);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoSenhaFraca()
        {
            var dto = new AdicionarUsuarioDto
            {
                Nome = "Novo Usuario",
                Email = "novo2@teste.com",
                Senha = "abc"
            };

            var response = await Client.PostAsJsonAsync("/api/usuarios", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoEmailDuplicado()
        {
            await SeedUsuarioAsync(email: "duplicado@teste.com");

            var dto = new AdicionarUsuarioDto
            {
                Nome = "Outro Usuario",
                Email = "duplicado@teste.com",
                Senha = SenhaPadrao
            };

            var response = await Client.PostAsJsonAsync("/api/usuarios", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarUsuario_QuandoExistir()
        {
            var usuario = await SeedUsuarioAsync();
            var dto = new AtualizarUsuarioDto { Nome = "Nome Novo", Email = "novoemail@teste.com" };

            var response = await Client.PutAsJsonAsync($"/api/usuarios/{usuario.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/usuarios/{usuario.Id}");
            var atualizado = await consulta.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.AreEqual("Nome Novo", atualizado!.Nome);
            Assert.AreEqual("novoemail@teste.com", atualizado.Email);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var dto = new AtualizarUsuarioDto { Nome = "Qualquer", Email = "qualquer@teste.com" };

            var response = await Client.PutAsJsonAsync("/api/usuarios/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Login_DeveRetornarUsuario_QuandoCredenciaisValidas()
        {
            var usuario = await SeedUsuarioAsync(email: "login@teste.com");
            var dto = new LoginUsuarioDto { Email = "login@teste.com", Senha = SenhaPadrao };

            var response = await Client.PostAsJsonAsync("/api/auth/login", dto);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.IsNotNull(resultado);
            Assert.AreEqual(usuario.Id, resultado!.Usuario.Id);
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultado.Token));
        }

        [TestMethod]
        public async Task Login_DeveRetornarUnauthorized_QuandoSenhaIncorreta()
        {
            await SeedUsuarioAsync(email: "login2@teste.com");
            var dto = new LoginUsuarioDto { Email = "login2@teste.com", Senha = "SenhaErrada123" };

            var response = await Client.PostAsJsonAsync("/api/auth/login", dto);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task Login_DeveRetornarUnauthorized_QuandoEmailNaoExistir()
        {
            var dto = new LoginUsuarioDto { Email = "naoexiste@teste.com", Senha = SenhaPadrao };

            var response = await Client.PostAsJsonAsync("/api/auth/login", dto);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task AlterarSenha_DeveAlterarSenha_QuandoSenhaAtualCorreta()
        {
            var usuario = await SeedUsuarioAsync(email: "senha@teste.com");
            var dto = new AlterarSenhaUsuarioDto { SenhaAtual = SenhaPadrao, NovaSenha = "NovaSenha123" };

            var response = await Client.PatchAsJsonAsync($"/api/usuarios/{usuario.Id}/senha", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var login = await Client.PostAsJsonAsync("/api/auth/login",
                new LoginUsuarioDto { Email = "senha@teste.com", Senha = "NovaSenha123" });
            Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);
        }

        [TestMethod]
        public async Task AlterarSenha_DeveRetornarBadRequest_QuandoSenhaAtualIncorreta()
        {
            var usuario = await SeedUsuarioAsync();
            var dto = new AlterarSenhaUsuarioDto { SenhaAtual = "SenhaErrada123", NovaSenha = "NovaSenha123" };

            var response = await Client.PatchAsJsonAsync($"/api/usuarios/{usuario.Id}/senha", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverUsuario_QuandoExistir()
        {
            var usuario = await SeedUsuarioAsync();

            var response = await Client.DeleteAsync($"/api/usuarios/{usuario.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/usuarios/{usuario.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/usuarios/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarUsuario_QuandoExistir()
        {
            var usuario = await SeedUsuarioAsync(ativo: true);

            var response = await Client.PatchAsync($"/api/usuarios/{usuario.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/usuarios/{usuario.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<UsuarioDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/usuarios/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
