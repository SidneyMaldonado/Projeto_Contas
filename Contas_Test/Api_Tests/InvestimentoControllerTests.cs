using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class InvestimentoControllerTests : ApiTestBase
    {
        private Task<Usuario> SeedOutroUsuarioAsync() =>
            SeedAsync(new Usuario { Nome = "Outro UsuÃ¡rio", Email = $"{Guid.NewGuid()}@teste.com", Senha = "hash", Ativo = true });

        private Task<Carteira> SeedCarteiraAsync(int idUsuario) =>
            SeedAsync(new Carteira { IdUsuario = idUsuario, Nome = "Carteira Renda Fixa", Ativo = true });

        private async Task<Investimento> SeedInvestimentoAsync(
            int idCarteira,
            string nome = "Tesouro Selic",
            decimal quantidade = 10m,
            decimal cotacao = 100m,
            bool ativo = true)
        {
            var investimento = new Investimento
            {
                IdCarteira = idCarteira,
                Nome = nome,
                Quantidade = quantidade,
                Cotacao = cotacao,
                Ativo = ativo
            };

            return await SeedAsync(investimento);
        }

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeInvestimentos()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);
            await SeedInvestimentoAsync(carteira.Id);

            var response = await Client.GetAsync("/api/investimentos");
            response.EnsureSuccessStatusCode();

            var investimentos = await response.Content.ReadFromJsonAsync<List<InvestimentoDto>>();

            Assert.IsNotNull(investimentos);
            Assert.IsNotEmpty(investimentos);
        }

        [TestMethod]
        public async Task ObterTodos_NaoDeveRetornarInvestimentoDeOutroUsuario()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);
            await SeedInvestimentoAsync(carteira.Id, "Meu Investimento");

            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);
            var investimentoAlheio = await SeedInvestimentoAsync(carteiraAlheia.Id, "Investimento Alheio");

            var response = await Client.GetAsync("/api/investimentos");
            var investimentos = await response.Content.ReadFromJsonAsync<List<InvestimentoDto>>();

            Assert.IsFalse(investimentos!.Exists(i => i.Id == investimentoAlheio.Id));
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarInvestimento_QuandoExistir()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);
            var investimento = await SeedInvestimentoAsync(carteira.Id, "AÃ§Ã£o XPTO", 15m, 50m);

            var response = await Client.GetAsync($"/api/investimentos/{investimento.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<InvestimentoDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(investimento.Id, dto!.Id);
            Assert.AreEqual("AÃ§Ã£o XPTO", dto.Nome);
            Assert.AreEqual(15m, dto.Quantidade);
            Assert.AreEqual(50m, dto.Cotacao);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/investimentos/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoInvestimentoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);
            var investimentoAlheio = await SeedInvestimentoAsync(carteiraAlheia.Id);

            var response = await Client.GetAsync($"/api/investimentos/{investimentoAlheio.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarInvestimento_QuandoValido()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);

            var dto = new AdicionarInvestimentoDto
            {
                IdCarteira = carteira.Id,
                Nome = "Fundo ImobiliÃ¡rio",
                Quantidade = 30m,
                Cotacao = 120m,
                Observacao = "Compra mensal"
            };

            var response = await Client.PostAsJsonAsync("/api/investimentos", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criado = await response.Content.ReadFromJsonAsync<InvestimentoDto>();
            Assert.IsNotNull(criado);
            Assert.AreEqual("Fundo ImobiliÃ¡rio", criado!.Nome);
            Assert.AreNotEqual(0, criado.Id);
            Assert.IsTrue(criado.Ativo);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarNotFound_QuandoCarteiraNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);

            var dto = new AdicionarInvestimentoDto
            {
                IdCarteira = carteiraAlheia.Id,
                Nome = "Tentativa de InvasÃ£o",
                Quantidade = 10m,
                Cotacao = 10m
            };

            var response = await Client.PostAsJsonAsync("/api/investimentos", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoQuantidadeInvalida()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);

            var dto = new AdicionarInvestimentoDto
            {
                IdCarteira = carteira.Id,
                Nome = "Investimento InvÃ¡lido",
                Quantidade = 0m,
                Cotacao = 10m
            };

            var response = await Client.PostAsJsonAsync("/api/investimentos", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarInvestimento_QuandoExistir()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);
            var investimento = await SeedInvestimentoAsync(carteira.Id, "Nome Antigo", 10m, 100m);

            var dto = new AtualizarInvestimentoDto
            {
                IdCarteira = carteira.Id,
                Nome = "Nome Novo",
                Quantidade = 25m,
                Cotacao = 150m
            };

            var response = await Client.PutAsJsonAsync($"/api/investimentos/{investimento.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/investimentos/{investimento.Id}");
            var atualizado = await consulta.Content.ReadFromJsonAsync<InvestimentoDto>();
            Assert.AreEqual("Nome Novo", atualizado!.Nome);
            Assert.AreEqual(25m, atualizado.Quantidade);
            Assert.AreEqual(150m, atualizado.Cotacao);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);

            var dto = new AtualizarInvestimentoDto
            {
                IdCarteira = carteira.Id,
                Nome = "Qualquer",
                Quantidade = 10m,
                Cotacao = 10m
            };

            var response = await Client.PutAsJsonAsync("/api/investimentos/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoInvestimentoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);
            var investimentoAlheio = await SeedInvestimentoAsync(carteiraAlheia.Id);

            var dto = new AtualizarInvestimentoDto
            {
                IdCarteira = carteiraAlheia.Id,
                Nome = "InvasÃ£o",
                Quantidade = 10m,
                Cotacao = 10m
            };

            var response = await Client.PutAsJsonAsync($"/api/investimentos/{investimentoAlheio.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNovaCarteiraNaoPertenceAoUsuarioAtual()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);
            var investimento = await SeedInvestimentoAsync(carteira.Id);

            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);

            var dto = new AtualizarInvestimentoDto
            {
                IdCarteira = carteiraAlheia.Id,
                Nome = "Tentativa de Mover para Carteira Alheia",
                Quantidade = 10m,
                Cotacao = 10m
            };

            var response = await Client.PutAsJsonAsync($"/api/investimentos/{investimento.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverInvestimento_QuandoExistir()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);
            var investimento = await SeedInvestimentoAsync(carteira.Id);

            var response = await Client.DeleteAsync($"/api/investimentos/{investimento.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/investimentos/{investimento.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/investimentos/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoInvestimentoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);
            var investimentoAlheio = await SeedInvestimentoAsync(carteiraAlheia.Id);

            var response = await Client.DeleteAsync($"/api/investimentos/{investimentoAlheio.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarInvestimento_QuandoExistir()
        {
            var carteira = await SeedCarteiraAsync(CurrentUser.Id);
            var investimento = await SeedInvestimentoAsync(carteira.Id, ativo: true);

            var response = await Client.PatchAsync($"/api/investimentos/{investimento.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/investimentos/{investimento.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<InvestimentoDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/investimentos/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoInvestimentoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var carteiraAlheia = await SeedCarteiraAsync(outroUsuario.Id);
            var investimentoAlheio = await SeedInvestimentoAsync(carteiraAlheia.Id);

            var response = await Client.PatchAsync($"/api/investimentos/{investimentoAlheio.Id}/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
