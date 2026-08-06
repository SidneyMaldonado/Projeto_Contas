using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class CarteiraControllerTests : ApiTestBase
    {
        private Task<Usuario> SeedOutroUsuarioAsync() =>
            SeedAsync(new Usuario { Nome = "Outro UsuÃ¡rio", Email = $"{Guid.NewGuid()}@teste.com", Senha = "hash", Ativo = true });

        private Task<Carteira> SeedCarteiraAsync(int idUsuario, string nome = "Carteira Renda Fixa", bool ativo = true) =>
            SeedAsync(new Carteira
            {
                IdUsuario = idUsuario,
                Nome = nome,
                Ativo = ativo
            });

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeCarteiras()
        {
            await SeedCarteiraAsync(CurrentUser.Id);

            var response = await Client.GetAsync("/api/carteiras");
            response.EnsureSuccessStatusCode();

            var carteiras = await response.Content.ReadFromJsonAsync<List<CarteiraDto>>();

            Assert.IsNotNull(carteiras);
            Assert.IsNotEmpty(carteiras);
        }

        [TestMethod]
        public async Task ObterTodos_NaoDeveRetornarCarteiraDeOutroUsuario()
        {
            await SeedCarteiraAsync(CurrentUser.Id, "Minha Carteira");
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id, "Carteira Alheia");

            var response = await Client.GetAsync("/api/carteiras");
            var carteiras = await response.Content.ReadFromJsonAsync<List<CarteiraDto>>();

            Assert.IsFalse(carteiras!.Exists(c => c.Id == carteiraAlheia.Id));
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarCarteira_QuandoExistir()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id, "Carteira AÃ§Ãµes");

            var response = await Client.GetAsync($"/api/carteiras/{carteira.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<CarteiraDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(carteira.Id, dto!.Id);
            Assert.AreEqual("Carteira AÃ§Ãµes", dto.Nome);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/carteiras/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoCarteiraNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);

            var response = await Client.GetAsync($"/api/carteiras/{carteiraAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarCarteira_QuandoValida()
        {
            var dto = new AdicionarCarteiraDto { Nome = "Carteira Internacional" };

            var response = await Client.PostAsJsonAsync("/api/carteiras", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<CarteiraDto>();
            Assert.IsNotNull(criada);
            Assert.AreEqual("Carteira Internacional", criada!.Nome);
            Assert.AreNotEqual(0, criada.Id);
            Assert.IsTrue(criada.Ativo);
        }

        [TestMethod]
        public async Task Adicionar_DeveAssociarCarteiraAoUsuarioAutenticado()
        {
            var dto = new AdicionarCarteiraDto { Nome = "Carteira do UsuÃ¡rio Atual" };

            var response = await Client.PostAsJsonAsync("/api/carteiras", dto);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<CarteiraDto>();
            Assert.AreEqual(CurrentUser.Id, criada!.IdUsuario);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoNomeVazio()
        {
            var dto = new AdicionarCarteiraDto { Nome = "" };

            var response = await Client.PostAsJsonAsync("/api/carteiras", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoNomeMenorQue3Caracteres()
        {
            var dto = new AdicionarCarteiraDto { Nome = "Ca" };

            var response = await Client.PostAsJsonAsync("/api/carteiras", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarCarteira_QuandoExistir()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id, "Nome Antigo");

            var dto = new AtualizarCarteiraDto { Nome = "Nome Novo" };

            var response = await Client.PutAsJsonAsync($"/api/carteiras/{carteira.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/carteiras/{carteira.Id}");
            var atualizada = await consulta.Content.ReadFromJsonAsync<CarteiraDto>();
            Assert.AreEqual("Nome Novo", atualizada!.Nome);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var dto = new AtualizarCarteiraDto { Nome = "Qualquer" };

            var response = await Client.PutAsJsonAsync("/api/carteiras/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoCarteiraNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);

            var dto = new AtualizarCarteiraDto { Nome = "InvasÃ£o" };

            var response = await Client.PutAsJsonAsync($"/api/carteiras/{carteiraAlheia.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverCarteira_QuandoExistir()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);

            var response = await Client.DeleteAsync($"/api/carteiras/{carteira.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/carteiras/{carteira.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/carteiras/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoCarteiraNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);

            var response = await Client.DeleteAsync($"/api/carteiras/{carteiraAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarCarteira_QuandoExistir()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id, ativo: true);

            var response = await Client.PatchAsync($"/api/carteiras/{carteira.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/carteiras/{carteira.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<CarteiraDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/carteiras/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoCarteiraNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);

            var response = await Client.PatchAsync($"/api/carteiras/{carteiraAlheia.Id}/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
