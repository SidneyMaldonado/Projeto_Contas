using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Core.Dto;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class AutenticacaoTests : ApiTestBase
    {
        [TestMethod]
        public async Task AcessarEndpointProtegido_SemToken_DeveRetornarUnauthorized()
        {
            using var client = CreateAnonymousClient();

            var response = await client.GetAsync("/api/categorias");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task AcessarEndpointProtegido_ComTokenInvalido_DeveRetornarUnauthorized()
        {
            using var client = CreateAnonymousClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "token-invalido");

            var response = await client.GetAsync("/api/categorias");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [TestMethod]
        public async Task Registrar_DeveFuncionarSemToken()
        {
            using var client = CreateAnonymousClient();
            var dto = new AdicionarUsuarioDto { Nome = "Novo Usuario", Email = $"{Guid.NewGuid()}@teste.com", Senha = "Senha123" };

            var response = await client.PostAsJsonAsync("/api/usuarios", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [TestMethod]
        public async Task Login_DeveFuncionarSemToken_ERetornarTokenValido()
        {
            using var client = CreateAnonymousClient();
            var email = $"{Guid.NewGuid()}@teste.com";

            await client.PostAsJsonAsync("/api/usuarios", new AdicionarUsuarioDto
            {
                Nome = "Usuario Login",
                Email = email,
                Senha = "Senha123"
            });

            var loginResponse = await client.PostAsJsonAsync("/api/usuarios/login", new LoginUsuarioDto
            {
                Email = email,
                Senha = "Senha123"
            });

            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

            var resultado = await loginResponse.Content.ReadFromJsonAsync<LoginResponseDto>();
            Assert.IsNotNull(resultado);
            Assert.IsFalse(string.IsNullOrWhiteSpace(resultado!.Token));

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", resultado.Token);
            var protegida = await client.GetAsync("/api/categorias");

            Assert.AreEqual(HttpStatusCode.OK, protegida.StatusCode);
        }
    }
}
