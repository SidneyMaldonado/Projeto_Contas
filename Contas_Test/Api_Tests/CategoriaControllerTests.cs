using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class CategoriaControllerTests : ApiTestBase
    {
        private Task<Categoria> SeedCategoriaAsync(string nome = "AlimentaÃ§Ã£o", bool ativo = true) =>
            SeedAsync(new Categoria { Nome = nome, Ativo = ativo });

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeCategorias()
        {
            await SeedCategoriaAsync();

            var response = await Client.GetAsync("/api/categorias");
            response.EnsureSuccessStatusCode();

            var categorias = await response.Content.ReadFromJsonAsync<List<CategoriaDto>>();

            Assert.IsNotNull(categorias);
            Assert.IsNotEmpty(categorias);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarCategoria_QuandoExistir()
        {
            var categoria = await SeedCategoriaAsync("Transporte");

            var response = await Client.GetAsync($"/api/categorias/{categoria.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<CategoriaDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(categoria.Id, dto!.Id);
            Assert.AreEqual("Transporte", dto.Nome);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/categorias/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarCategoria_QuandoValida()
        {
            var dto = new AdicionarCategoriaDto { Nome = "Lazer" };

            var response = await Client.PostAsJsonAsync("/api/categorias", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<CategoriaDto>();
            Assert.IsNotNull(criada);
            Assert.AreEqual("Lazer", criada!.Nome);
            Assert.AreNotEqual(0, criada.Id);
            Assert.IsTrue(criada.Ativo);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoNomeInvalido()
        {
            var dto = new AdicionarCategoriaDto { Nome = "ab" };

            var response = await Client.PostAsJsonAsync("/api/categorias", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarCategoria_QuandoExistir()
        {
            var categoria = await SeedCategoriaAsync("Nome Antigo");
            var dto = new AtualizarCategoriaDto { Nome = "Nome Novo" };

            var response = await Client.PutAsJsonAsync($"/api/categorias/{categoria.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/categorias/{categoria.Id}");
            var atualizada = await consulta.Content.ReadFromJsonAsync<CategoriaDto>();
            Assert.AreEqual("Nome Novo", atualizada!.Nome);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var dto = new AtualizarCategoriaDto { Nome = "Qualquer" };

            var response = await Client.PutAsJsonAsync("/api/categorias/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverCategoria_QuandoExistir()
        {
            var categoria = await SeedCategoriaAsync();

            var response = await Client.DeleteAsync($"/api/categorias/{categoria.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/categorias/{categoria.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/categorias/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarCategoria_QuandoExistir()
        {
            var categoria = await SeedCategoriaAsync(ativo: true);

            var response = await Client.PatchAsync($"/api/categorias/{categoria.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/categorias/{categoria.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<CategoriaDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/categorias/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
