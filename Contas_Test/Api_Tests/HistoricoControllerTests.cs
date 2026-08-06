using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class HistoricoControllerTests : ApiTestBase
    {
        private Task<Usuario> SeedOutroUsuarioAsync() =>
            SeedAsync(new Usuario { Nome = "Outro UsuÃ¡rio", Email = $"{Guid.NewGuid()}@teste.com", Senha = "hash", Ativo = true });

        private Task<Carteira> SeedCarteiraAsync(int idUsuario, string nome = "Carteira Principal") =>
            SeedAsync(new Carteira { IdUsuario = idUsuario, Nome = nome, Ativo = true });

        private Task<Investimento> SeedInvestimentoAsync(int idCarteira, string nome = "AÃ§Ã£o XYZ") =>
            SeedAsync(new Investimento
            {
                IdCarteira = idCarteira,
                Nome = nome,
                Quantidade = 10m,
                Cotacao = 25.50m,
                Observacao = "Investimento de teste",
                Ativo = true
            });

        private async Task<(Carteira Carteira, Investimento Investimento)> SeedDependenciasAsync(int idUsuario)
        {
            var carteira = await SeedCarteiraAsync(idUsuario);
            var investimento = await SeedInvestimentoAsync(carteira.Id);

            return (carteira, investimento);
        }

        private Task<Historico> SeedHistoricoAsync(
            int idInvestimento,
            string nomeInvestimento = "AÃ§Ã£o XYZ",
            decimal quantidade = 10m,
            decimal cotacao = 25.50m,
            string observacao = "Snapshot de teste",
            bool ativo = true)
        {
            return SeedAsync(new Historico
            {
                IdInvestimento = idInvestimento,
                NomeInvestimento = nomeInvestimento,
                Quantidade = quantidade,
                Cotacao = cotacao,
                Observacao = observacao,
                DataHistorico = DateTime.UtcNow,
                Ativo = ativo
            });
        }

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeHistoricos()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);
            await SeedHistoricoAsync(investimento.Id);

            var response = await Client.GetAsync("/api/historicos");
            response.EnsureSuccessStatusCode();

            var historicos = await response.Content.ReadFromJsonAsync<List<HistoricoDto>>();

            Assert.IsNotNull(historicos);
            Assert.IsNotEmpty(historicos);
        }

        [TestMethod]
        public async Task ObterTodos_NaoDeveRetornarHistoricoDeOutroUsuario()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);
            await SeedHistoricoAsync(investimento.Id, "Meu Ativo");

            var outroUsuario = await SeedOutroUsuarioAsync();
            var (_, investimentoAlheio) = await SeedDependenciasAsync(outroUsuario.Id);
            var historicoAlheio = await SeedHistoricoAsync(investimentoAlheio.Id, "Ativo Alheio");

            var response = await Client.GetAsync("/api/historicos");
            var historicos = await response.Content.ReadFromJsonAsync<List<HistoricoDto>>();

            Assert.IsFalse(historicos!.Exists(h => h.Id == historicoAlheio.Id));
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarHistorico_QuandoExistir()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);
            var historico = await SeedHistoricoAsync(investimento.Id, "Tesouro Selic", 20m, 100m, "Aporte mensal");

            var response = await Client.GetAsync($"/api/historicos/{historico.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<HistoricoDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(historico.Id, dto!.Id);
            Assert.AreEqual("Tesouro Selic", dto.NomeInvestimento);
            Assert.AreEqual(20m, dto.Quantidade);
            Assert.AreEqual(100m, dto.Cotacao);
            Assert.AreEqual("Aporte mensal", dto.Observacao);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/historicos/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoHistoricoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (_, investimentoAlheio) = await SeedDependenciasAsync(outroUsuario.Id);
            var historicoAlheio = await SeedHistoricoAsync(investimentoAlheio.Id);

            var response = await Client.GetAsync($"/api/historicos/{historicoAlheio.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarHistorico_QuandoValido()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AdicionarHistoricoDto
            {
                IdInvestimento = investimento.Id,
                NomeInvestimento = "AÃ§Ã£o XYZ",
                Quantidade = 15m,
                Cotacao = 30m,
                Observacao = "Compra adicional"
            };

            var response = await Client.PostAsJsonAsync("/api/historicos", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criado = await response.Content.ReadFromJsonAsync<HistoricoDto>();
            Assert.IsNotNull(criado);
            Assert.AreEqual("AÃ§Ã£o XYZ", criado!.NomeInvestimento);
            Assert.AreNotEqual(0, criado.Id);
            Assert.IsTrue(criado.Ativo);
            Assert.AreNotEqual(default, criado.DataHistorico);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarNotFound_QuandoInvestimentoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (_, investimentoAlheio) = await SeedDependenciasAsync(outroUsuario.Id);

            var dto = new AdicionarHistoricoDto
            {
                IdInvestimento = investimentoAlheio.Id,
                NomeInvestimento = "Tentativa de InvasÃ£o",
                Quantidade = 1m,
                Cotacao = 1m,
                Observacao = "InvasÃ£o"
            };

            var response = await Client.PostAsJsonAsync("/api/historicos", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoObservacaoVazia()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AdicionarHistoricoDto
            {
                IdInvestimento = investimento.Id,
                NomeInvestimento = "AÃ§Ã£o XYZ",
                Quantidade = 15m,
                Cotacao = 30m,
                Observacao = ""
            };

            var response = await Client.PostAsJsonAsync("/api/historicos", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoQuantidadeNegativa()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AdicionarHistoricoDto
            {
                IdInvestimento = investimento.Id,
                NomeInvestimento = "AÃ§Ã£o XYZ",
                Quantidade = -1m,
                Cotacao = 30m,
                Observacao = "ObservaÃ§Ã£o vÃ¡lida"
            };

            var response = await Client.PostAsJsonAsync("/api/historicos", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarHistorico_QuandoExistir()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);
            var historico = await SeedHistoricoAsync(investimento.Id, "Nome Antigo", 10m, 20m, "ObservaÃ§Ã£o antiga");

            var dto = new AtualizarHistoricoDto
            {
                IdInvestimento = investimento.Id,
                NomeInvestimento = "Nome Novo",
                Quantidade = 50m,
                Cotacao = 35m,
                Observacao = "ObservaÃ§Ã£o nova"
            };

            var response = await Client.PutAsJsonAsync($"/api/historicos/{historico.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/historicos/{historico.Id}");
            var atualizado = await consulta.Content.ReadFromJsonAsync<HistoricoDto>();
            Assert.AreEqual("Nome Novo", atualizado!.NomeInvestimento);
            Assert.AreEqual(50m, atualizado.Quantidade);
            Assert.AreEqual(35m, atualizado.Cotacao);
            Assert.AreEqual("ObservaÃ§Ã£o nova", atualizado.Observacao);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AtualizarHistoricoDto
            {
                IdInvestimento = investimento.Id,
                NomeInvestimento = "Qualquer",
                Quantidade = 1m,
                Cotacao = 1m,
                Observacao = "Qualquer"
            };

            var response = await Client.PutAsJsonAsync("/api/historicos/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoHistoricoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (_, investimentoAlheio) = await SeedDependenciasAsync(outroUsuario.Id);
            var historicoAlheio = await SeedHistoricoAsync(investimentoAlheio.Id);

            var dto = new AtualizarHistoricoDto
            {
                IdInvestimento = investimentoAlheio.Id,
                NomeInvestimento = "InvasÃ£o",
                Quantidade = 1m,
                Cotacao = 1m,
                Observacao = "InvasÃ£o"
            };

            var response = await Client.PutAsJsonAsync($"/api/historicos/{historicoAlheio.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverHistorico_QuandoExistir()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);
            var historico = await SeedHistoricoAsync(investimento.Id);

            var response = await Client.DeleteAsync($"/api/historicos/{historico.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/historicos/{historico.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/historicos/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoHistoricoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (_, investimentoAlheio) = await SeedDependenciasAsync(outroUsuario.Id);
            var historicoAlheio = await SeedHistoricoAsync(investimentoAlheio.Id);

            var response = await Client.DeleteAsync($"/api/historicos/{historicoAlheio.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarHistorico_QuandoExistir()
        {
            var (_, investimento) = await SeedDependenciasAsync(CurrentUser.Id);
            var historico = await SeedHistoricoAsync(investimento.Id, ativo: true);

            var response = await Client.PatchAsync($"/api/historicos/{historico.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/historicos/{historico.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<HistoricoDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/historicos/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoHistoricoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (_, investimentoAlheio) = await SeedDependenciasAsync(outroUsuario.Id);
            var historicoAlheio = await SeedHistoricoAsync(investimentoAlheio.Id);

            var response = await Client.PatchAsync($"/api/historicos/{historicoAlheio.Id}/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Isolamento_UsuarioNaoDeveAcessarHistoricoCujoInvestimentoCarteiraPertenceAOutroUsuario()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id, "Carteira Alheia");
            var investimentoAlheio = await SeedInvestimentoAsync(carteiraAlheia.Id, "Ativo Alheio");
            var historicoAlheio = await SeedHistoricoAsync(investimentoAlheio.Id, "Ativo Alheio");

            var getResponse = await Client.GetAsync($"/api/historicos/{historicoAlheio.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);

            var putResponse = await Client.PutAsJsonAsync($"/api/historicos/{historicoAlheio.Id}", new AtualizarHistoricoDto
            {
                IdInvestimento = investimentoAlheio.Id,
                NomeInvestimento = "InvasÃ£o",
                Quantidade = 1m,
                Cotacao = 1m,
                Observacao = "InvasÃ£o"
            });
            Assert.AreEqual(HttpStatusCode.NotFound, putResponse.StatusCode);

            var deleteResponse = await Client.DeleteAsync($"/api/historicos/{historicoAlheio.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        }
    }
}
