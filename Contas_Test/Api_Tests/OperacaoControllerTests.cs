using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class OperacaoControllerTests : ApiTestBase
    {
        private Task<Usuario> SeedOutroUsuarioAsync() =>
            SeedAsync(new Usuario { Nome = "Outro UsuÃ¡rio", Email = $"{Guid.NewGuid()}@teste.com", Senha = "hash", Ativo = true });

        private Task<Carteira> SeedCarteiraAsync(int idUsuario) =>
            SeedAsync(new Carteira { IdUsuario = idUsuario, Nome = "Carteira Principal", Ativo = true });

        private Task<Investimento> SeedInvestimentoAsync(int idCarteira) =>
            SeedAsync(new Investimento
            {
                IdCarteira = idCarteira,
                Nome = "AÃ§Ã£o XYZ",
                Quantidade = 100m,
                Cotacao = 25m,
                Ativo = true
            });

        private async Task<Investimento> SeedDependenciasAsync(int idUsuario)
        {
            var carteira = await SeedCarteiraAsync(idUsuario);
            var investimento = await SeedInvestimentoAsync(carteira.Id);
            return investimento;
        }

        private Task<Operacao> SeedOperacaoAsync(
            int idInvestimento, bool compra = true, int quantidade = 10, decimal valor = 100m, bool ativo = true) =>
            SeedAsync(new Operacao
            {
                IdInvestimento = idInvestimento,
                Compra = compra,
                DataOperacao = DateTime.Today,
                Quantidade = quantidade,
                ValorOperacao = valor,
                Ativo = ativo
            });

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeOperacoes()
        {
            var investimento = await SeedDependenciasAsync(CurrentUser.Id);
            await SeedOperacaoAsync(investimento.Id);

            var response = await Client.GetAsync("/api/operacoes");
            response.EnsureSuccessStatusCode();

            var operacoes = await response.Content.ReadFromJsonAsync<List<OperacaoDto>>();

            Assert.IsNotNull(operacoes);
            Assert.IsNotEmpty(operacoes);
        }

        [TestMethod]
        public async Task ObterTodos_NaoDeveRetornarOperacaoDeOutroUsuario()
        {
            var investimento = await SeedDependenciasAsync(CurrentUser.Id);
            await SeedOperacaoAsync(investimento.Id, quantidade: 5);

            var outroUsuario = await SeedOutroUsuarioAsync();
            var investimentoAlheio = await SeedDependenciasAsync(outroUsuario.Id);
            var operacaoAlheia = await SeedOperacaoAsync(investimentoAlheio.Id, quantidade: 50);

            var response = await Client.GetAsync("/api/operacoes");
            var operacoes = await response.Content.ReadFromJsonAsync<List<OperacaoDto>>();

            Assert.IsFalse(operacoes!.Exists(o => o.Id == operacaoAlheia.Id));
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarOperacao_QuandoExistir()
        {
            var investimento = await SeedDependenciasAsync(CurrentUser.Id);
            var operacao = await SeedOperacaoAsync(investimento.Id, quantidade: 20, valor: 300m);

            var response = await Client.GetAsync($"/api/operacoes/{operacao.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<OperacaoDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(operacao.Id, dto!.Id);
            Assert.AreEqual(20, dto.Quantidade);
            Assert.AreEqual(300m, dto.ValorOperacao);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/operacoes/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoInvestimentoPertenceAOutroUsuario()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var investimentoAlheio = await SeedDependenciasAsync(outroUsuario.Id);
            var operacaoAlheia = await SeedOperacaoAsync(investimentoAlheio.Id);

            var response = await Client.GetAsync($"/api/operacoes/{operacaoAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarOperacao_QuandoValida()
        {
            var investimento = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AdicionarOperacaoDto
            {
                IdInvestimento = investimento.Id,
                Compra = true,
                DataOperacao = DateTime.Today,
                Quantidade = 30,
                ValorOperacao = 450m
            };

            var response = await Client.PostAsJsonAsync("/api/operacoes", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<OperacaoDto>();
            Assert.IsNotNull(criada);
            Assert.AreEqual(30, criada!.Quantidade);
            Assert.AreNotEqual(0, criada.Id);
            Assert.IsTrue(criada.Ativo);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarNotFound_QuandoInvestimentoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var investimentoAlheio = await SeedDependenciasAsync(outroUsuario.Id);

            var dto = new AdicionarOperacaoDto
            {
                IdInvestimento = investimentoAlheio.Id,
                Compra = true,
                DataOperacao = DateTime.Today,
                Quantidade = 10,
                ValorOperacao = 100m
            };

            var response = await Client.PostAsJsonAsync("/api/operacoes", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoQuantidadeInvalida()
        {
            var investimento = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AdicionarOperacaoDto
            {
                IdInvestimento = investimento.Id,
                Compra = true,
                DataOperacao = DateTime.Today,
                Quantidade = 0,
                ValorOperacao = 100m
            };

            var response = await Client.PostAsJsonAsync("/api/operacoes", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarOperacao_QuandoExistir()
        {
            var investimento = await SeedDependenciasAsync(CurrentUser.Id);
            var operacao = await SeedOperacaoAsync(investimento.Id, quantidade: 10, valor: 100m);

            var dto = new AtualizarOperacaoDto
            {
                IdInvestimento = investimento.Id,
                Compra = false,
                DataOperacao = DateTime.Today,
                Quantidade = 40,
                ValorOperacao = 600m
            };

            var response = await Client.PutAsJsonAsync($"/api/operacoes/{operacao.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/operacoes/{operacao.Id}");
            var atualizada = await consulta.Content.ReadFromJsonAsync<OperacaoDto>();
            Assert.AreEqual(40, atualizada!.Quantidade);
            Assert.AreEqual(600m, atualizada.ValorOperacao);
            Assert.IsFalse(atualizada.Compra);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var investimento = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AtualizarOperacaoDto
            {
                IdInvestimento = investimento.Id,
                Compra = true,
                DataOperacao = DateTime.Today,
                Quantidade = 10,
                ValorOperacao = 100m
            };

            var response = await Client.PutAsJsonAsync("/api/operacoes/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoOperacaoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var investimentoAlheio = await SeedDependenciasAsync(outroUsuario.Id);
            var operacaoAlheia = await SeedOperacaoAsync(investimentoAlheio.Id);

            var dto = new AtualizarOperacaoDto
            {
                IdInvestimento = investimentoAlheio.Id,
                Compra = true,
                DataOperacao = DateTime.Today,
                Quantidade = 10,
                ValorOperacao = 100m
            };

            var response = await Client.PutAsJsonAsync($"/api/operacoes/{operacaoAlheia.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverOperacao_QuandoExistir()
        {
            var investimento = await SeedDependenciasAsync(CurrentUser.Id);
            var operacao = await SeedOperacaoAsync(investimento.Id);

            var response = await Client.DeleteAsync($"/api/operacoes/{operacao.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/operacoes/{operacao.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/operacoes/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoOperacaoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var investimentoAlheio = await SeedDependenciasAsync(outroUsuario.Id);
            var operacaoAlheia = await SeedOperacaoAsync(investimentoAlheio.Id);

            var response = await Client.DeleteAsync($"/api/operacoes/{operacaoAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarOperacao_QuandoExistir()
        {
            var investimento = await SeedDependenciasAsync(CurrentUser.Id);
            var operacao = await SeedOperacaoAsync(investimento.Id, ativo: true);

            var response = await Client.PatchAsync($"/api/operacoes/{operacao.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/operacoes/{operacao.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<OperacaoDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/operacoes/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoOperacaoNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var investimentoAlheio = await SeedDependenciasAsync(outroUsuario.Id);
            var operacaoAlheia = await SeedOperacaoAsync(investimentoAlheio.Id);

            var response = await Client.PatchAsync($"/api/operacoes/{operacaoAlheia.Id}/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoUsuarioATentaAcessarOperacaoDeUsuarioB()
        {
            // Teste de isolamento em dois nÃ­veis: Operacao -> Investimento -> Carteira -> Usuario.
            var usuarioB = await SeedOutroUsuarioAsync();
            var investimentoDoB = await SeedDependenciasAsync(usuarioB.Id);
            var operacaoDoB = await SeedOperacaoAsync(investimentoDoB.Id, quantidade: 99, valor: 999m);

            // Client autenticado Ã© o usuÃ¡rio A (CurrentUser).
            var response = await Client.GetAsync($"/api/operacoes/{operacaoDoB.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
