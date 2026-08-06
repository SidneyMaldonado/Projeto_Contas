using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class DividaControllerTests : ApiTestBase
    {
        private Task<Usuario> SeedOutroUsuarioAsync() =>
            SeedAsync(new Usuario { Nome = "Outro UsuÃ¡rio", Email = $"{Guid.NewGuid()}@teste.com", Senha = "hash", Ativo = true });

        private Task<Divida> SeedDividaAsync(int idUsuario, string nome = "Financiamento", decimal valor = 1000m, bool ativo = true)
        {
            var dataVencimento = DateTime.Today.AddMonths(1);

            return SeedAsync(new Divida
            {
                IdUsuario = idUsuario,
                Nome = nome,
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 12,
                Valor = valor,
                Ativo = ativo
            });
        }

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeDividas()
        {
            await SeedDividaAsync(CurrentUser.Id);

            var response = await Client.GetAsync("/api/dividas");
            response.EnsureSuccessStatusCode();

            var dividas = await response.Content.ReadFromJsonAsync<List<DividaDto>>();

            Assert.IsNotNull(dividas);
            Assert.IsNotEmpty(dividas);
        }

        [TestMethod]
        public async Task ObterTodos_NaoDeveRetornarDividaDeOutroUsuario()
        {
            await SeedDividaAsync(CurrentUser.Id, "Minha DÃ­vida");
            var outroUsuario = await SeedOutroUsuarioAsync();
            var dividaAlheia = await SeedDividaAsync(outroUsuario.Id, "DÃ­vida Alheia");

            var response = await Client.GetAsync("/api/dividas");
            var dividas = await response.Content.ReadFromJsonAsync<List<DividaDto>>();

            Assert.IsFalse(dividas!.Exists(d => d.Id == dividaAlheia.Id));
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarDivida_QuandoExistir()
        {
            var divida = await SeedDividaAsync(CurrentUser.Id, "CartÃ£o de CrÃ©dito", 2000m);

            var response = await Client.GetAsync($"/api/dividas/{divida.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<DividaDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(divida.Id, dto!.Id);
            Assert.AreEqual("CartÃ£o de CrÃ©dito", dto.Nome);
            Assert.AreEqual(2000m, dto.Valor);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/dividas/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoDividaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var dividaAlheia = await SeedDividaAsync(outroUsuario.Id);

            var response = await Client.GetAsync($"/api/dividas/{dividaAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarDivida_QuandoValida()
        {
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                Nome = "EmprÃ©stimo Pessoal",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 6,
                Valor = 500m
            };

            var response = await Client.PostAsJsonAsync("/api/dividas", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<DividaDto>();
            Assert.IsNotNull(criada);
            Assert.AreEqual("EmprÃ©stimo Pessoal", criada!.Nome);
            Assert.AreNotEqual(0, criada.Id);
            Assert.IsTrue(criada.Ativo);
        }

        [TestMethod]
        public async Task Adicionar_DeveIgnorarIdUsuarioDoDto_EUsarUsuarioAutenticado()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = outroUsuario.Id,
                Nome = "DÃ­vida Forjada",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 1,
                Valor = 100m
            };

            var response = await Client.PostAsJsonAsync("/api/dividas", dto);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<DividaDto>();
            Assert.AreEqual(CurrentUser.Id, criada!.IdUsuario);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoValorInvalido()
        {
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                Nome = "DÃ­vida InvÃ¡lida",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 1,
                Valor = 0m
            };

            var response = await Client.PostAsJsonAsync("/api/dividas", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoDataNoPassado()
        {
            var dataVencimento = DateTime.Today.AddDays(-5);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                Nome = "DÃ­vida Vencida",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 1,
                Valor = 100m
            };

            var response = await Client.PostAsJsonAsync("/api/dividas", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarDivida_QuandoExistir()
        {
            var divida = await SeedDividaAsync(CurrentUser.Id, "Nome Antigo", 1000m);
            var dataVencimento = DateTime.Today.AddMonths(2);

            var dto = new AtualizarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                Nome = "Nome Novo",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 10,
                Valor = 1500m
            };

            var response = await Client.PutAsJsonAsync($"/api/dividas/{divida.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/dividas/{divida.Id}");
            var atualizada = await consulta.Content.ReadFromJsonAsync<DividaDto>();
            Assert.AreEqual("Nome Novo", atualizada!.Nome);
            Assert.AreEqual(1500m, atualizada.Valor);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AtualizarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                Nome = "Qualquer",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 1,
                Valor = 100m
            };

            var response = await Client.PutAsJsonAsync("/api/dividas/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoDividaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var dividaAlheia = await SeedDividaAsync(outroUsuario.Id);
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AtualizarDividaDto
            {
                IdUsuario = outroUsuario.Id,
                Nome = "InvasÃ£o",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 1,
                Valor = 100m
            };

            var response = await Client.PutAsJsonAsync($"/api/dividas/{dividaAlheia.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverDivida_QuandoExistir()
        {
            var divida = await SeedDividaAsync(CurrentUser.Id);

            var response = await Client.DeleteAsync($"/api/dividas/{divida.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/dividas/{divida.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/dividas/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoDividaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var dividaAlheia = await SeedDividaAsync(outroUsuario.Id);

            var response = await Client.DeleteAsync($"/api/dividas/{dividaAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarDivida_QuandoExistir()
        {
            var divida = await SeedDividaAsync(CurrentUser.Id, ativo: true);

            var response = await Client.PatchAsync($"/api/dividas/{divida.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/dividas/{divida.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<DividaDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/dividas/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoDividaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var dividaAlheia = await SeedDividaAsync(outroUsuario.Id);

            var response = await Client.PatchAsync($"/api/dividas/{dividaAlheia.Id}/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
