using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class CredorControllerTests : ApiTestBase
    {
        private Task<Credor> SeedCredorAsync(string nome = "Banco XYZ", bool ativo = true) =>
            SeedAsync(new Credor { Nome = nome, Ativo = ativo });

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeCredores()
        {
            await SeedCredorAsync();

            var response = await Client.GetAsync("/api/credores");
            response.EnsureSuccessStatusCode();

            var credores = await response.Content.ReadFromJsonAsync<List<CredorDto>>();

            Assert.IsNotNull(credores);
            Assert.IsNotEmpty(credores);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarCredor_QuandoExistir()
        {
            var credor = await SeedCredorAsync("CartÃ£o ABC");

            var response = await Client.GetAsync($"/api/credores/{credor.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<CredorDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(credor.Id, dto!.Id);
            Assert.AreEqual("CartÃ£o ABC", dto.Nome);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/credores/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarCredor_QuandoValido()
        {
            var dto = new AdicionarCredorDto { Nome = "Financeira XPTO", Observacoes = "CartÃ£o de crÃ©dito" };

            var response = await Client.PostAsJsonAsync("/api/credores", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criado = await response.Content.ReadFromJsonAsync<CredorDto>();
            Assert.IsNotNull(criado);
            Assert.AreEqual("Financeira XPTO", criado!.Nome);
            Assert.AreNotEqual(0, criado.Id);
            Assert.IsTrue(criado.Ativo);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoNomeInvalido()
        {
            var dto = new AdicionarCredorDto { Nome = "ab" };

            var response = await Client.PostAsJsonAsync("/api/credores", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarCredor_QuandoExistir()
        {
            var credor = await SeedCredorAsync("Nome Antigo");
            var dto = new AtualizarCredorDto { Nome = "Nome Novo" };

            var response = await Client.PutAsJsonAsync($"/api/credores/{credor.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/credores/{credor.Id}");
            var atualizado = await consulta.Content.ReadFromJsonAsync<CredorDto>();
            Assert.AreEqual("Nome Novo", atualizado!.Nome);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var dto = new AtualizarCredorDto { Nome = "Qualquer" };

            var response = await Client.PutAsJsonAsync("/api/credores/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverCredor_QuandoExistir()
        {
            var credor = await SeedCredorAsync();

            var response = await Client.DeleteAsync($"/api/credores/{credor.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/credores/{credor.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/credores/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarCredor_QuandoExistir()
        {
            var credor = await SeedCredorAsync(ativo: true);

            var response = await Client.PatchAsync($"/api/credores/{credor.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/credores/{credor.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<CredorDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/credores/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
