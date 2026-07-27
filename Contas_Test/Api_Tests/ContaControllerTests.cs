using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Core.Dto;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class ContaControllerTests : ApiTestBase
    {
        private Task<Usuario> SeedOutroUsuarioAsync() =>
            SeedAsync(new Usuario { Nome = "Outro Usuário", Email = $"{Guid.NewGuid()}@teste.com", Senha = "hash", Ativo = true });

        private Task<Conta> SeedContaAsync(int idUsuario, string nome = "Conta Corrente", decimal saldo = 100m, bool ativo = true) =>
            SeedAsync(new Conta { IdUsuario = idUsuario, Nome = nome, Saldo = saldo, Ativo = ativo });

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeContas()
        {
            await SeedContaAsync(CurrentUser.Id);

            var response = await Client.GetAsync("/api/contas");
            response.EnsureSuccessStatusCode();

            var contas = await response.Content.ReadFromJsonAsync<List<ContaDto>>();

            Assert.IsNotNull(contas);
            Assert.IsNotEmpty(contas);
        }

        [TestMethod]
        public async Task ObterTodos_NaoDeveRetornarContaDeOutroUsuario()
        {
            await SeedContaAsync(CurrentUser.Id, "Minha Conta");
            var outroUsuario = await SeedOutroUsuarioAsync();
            var contaAlheia = await SeedContaAsync(outroUsuario.Id, "Conta Alheia");

            var response = await Client.GetAsync("/api/contas");
            response.EnsureSuccessStatusCode();

            var contas = await response.Content.ReadFromJsonAsync<List<ContaDto>>();

            Assert.IsFalse(contas!.Exists(c => c.Id == contaAlheia.Id));
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarConta_QuandoExistir()
        {
            var conta = await SeedContaAsync(CurrentUser.Id, "Poupança", 500m);

            var response = await Client.GetAsync($"/api/contas/{conta.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<ContaDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(conta.Id, dto!.Id);
            Assert.AreEqual("Poupança", dto.Nome);
            Assert.AreEqual(500m, dto.Saldo);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/contas/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoContaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var contaAlheia = await SeedContaAsync(outroUsuario.Id);

            var response = await Client.GetAsync($"/api/contas/{contaAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarConta_QuandoValida()
        {
            var dto = new AdicionarContaDto { IdUsuario = CurrentUser.Id, Nome = "Nova Conta", Saldo = 250m };

            var response = await Client.PostAsJsonAsync("/api/contas", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<ContaDto>();
            Assert.IsNotNull(criada);
            Assert.AreEqual("Nova Conta", criada!.Nome);
            Assert.AreEqual(250m, criada.Saldo);
            Assert.AreNotEqual(0, criada.Id);
            Assert.IsTrue(criada.Ativo);
        }

        [TestMethod]
        public async Task Adicionar_DeveIgnorarIdUsuarioDoDto_EUsarUsuarioAutenticado()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var dto = new AdicionarContaDto { IdUsuario = outroUsuario.Id, Nome = "Conta Forjada", Saldo = 100m };

            var response = await Client.PostAsJsonAsync("/api/contas", dto);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<ContaDto>();
            Assert.AreEqual(CurrentUser.Id, criada!.IdUsuario);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoSaldoNegativo()
        {
            var dto = new AdicionarContaDto { IdUsuario = CurrentUser.Id, Nome = "Conta Inválida", Saldo = -10m };

            var response = await Client.PostAsJsonAsync("/api/contas", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarConta_QuandoExistir()
        {
            var conta = await SeedContaAsync(CurrentUser.Id, "Nome Antigo", 100m);
            var dto = new AtualizarContaDto { IdUsuario = CurrentUser.Id, Nome = "Nome Novo", Saldo = 300m };

            var response = await Client.PutAsJsonAsync($"/api/contas/{conta.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/contas/{conta.Id}");
            var atualizada = await consulta.Content.ReadFromJsonAsync<ContaDto>();
            Assert.AreEqual("Nome Novo", atualizada!.Nome);
            Assert.AreEqual(300m, atualizada.Saldo);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var dto = new AtualizarContaDto { IdUsuario = CurrentUser.Id, Nome = "Qualquer", Saldo = 0m };

            var response = await Client.PutAsJsonAsync("/api/contas/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoContaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var contaAlheia = await SeedContaAsync(outroUsuario.Id);
            var dto = new AtualizarContaDto { IdUsuario = outroUsuario.Id, Nome = "Invasão", Saldo = 0m };

            var response = await Client.PutAsJsonAsync($"/api/contas/{contaAlheia.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task AtualizarSaldos_DeveAtualizarSaldosEmLote()
        {
            var contaA = await SeedContaAsync(CurrentUser.Id, "Conta A", 100m);
            var contaB = await SeedContaAsync(CurrentUser.Id, "Conta B", 200m);

            var saldos = new List<ContaResumoDto>
            {
                new() { Codigo = contaA.Id, Nome = contaA.Nome, Saldo = 150m },
                new() { Codigo = contaB.Id, Nome = contaB.Nome, Saldo = 250m }
            };

            var response = await Client.PutAsJsonAsync("/api/contas/saldos", saldos);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consultaA = await Client.GetAsync($"/api/contas/{contaA.Id}");
            var dtoA = await consultaA.Content.ReadFromJsonAsync<ContaDto>();
            var consultaB = await Client.GetAsync($"/api/contas/{contaB.Id}");
            var dtoB = await consultaB.Content.ReadFromJsonAsync<ContaDto>();

            Assert.AreEqual(150m, dtoA!.Saldo);
            Assert.AreEqual(250m, dtoB!.Saldo);
        }

        [TestMethod]
        public async Task AtualizarSaldos_NaoDeveAtualizarSaldoDeContaDeOutroUsuario()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var contaAlheia = await SeedContaAsync(outroUsuario.Id, "Conta Alheia", 100m);

            var saldos = new List<ContaResumoDto>
            {
                new() { Codigo = contaAlheia.Id, Nome = contaAlheia.Nome, Saldo = 999m }
            };

            var response = await Client.PutAsJsonAsync("/api/contas/saldos", saldos);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var tokenOutroUsuario = GenerateToken(outroUsuario);
            using var clienteOutroUsuario = CreateAnonymousClient();
            clienteOutroUsuario.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenOutroUsuario);

            var consulta = await clienteOutroUsuario.GetAsync($"/api/contas/{contaAlheia.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<ContaDto>();

            Assert.AreEqual(100m, dto!.Saldo);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverConta_QuandoExistir()
        {
            var conta = await SeedContaAsync(CurrentUser.Id);

            var response = await Client.DeleteAsync($"/api/contas/{conta.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/contas/{conta.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/contas/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoContaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var contaAlheia = await SeedContaAsync(outroUsuario.Id);

            var response = await Client.DeleteAsync($"/api/contas/{contaAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarConta_QuandoExistir()
        {
            var conta = await SeedContaAsync(CurrentUser.Id, ativo: true);

            var response = await Client.PatchAsync($"/api/contas/{conta.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/contas/{conta.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<ContaDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/contas/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoContaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var contaAlheia = await SeedContaAsync(outroUsuario.Id);

            var response = await Client.PatchAsync($"/api/contas/{contaAlheia.Id}/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ObterResumo_DeveRetornarApenasContasAtivasDoUsuarioAtual()
        {
            var ativa = await SeedContaAsync(CurrentUser.Id, "Conta Ativa", 100m, ativo: true);
            var inativa = await SeedContaAsync(CurrentUser.Id, "Conta Inativa", 200m, ativo: false);
            var outroUsuario = await SeedOutroUsuarioAsync();
            var contaAlheia = await SeedContaAsync(outroUsuario.Id, "Conta Alheia Ativa", 300m, ativo: true);

            var response = await Client.GetAsync("/api/contas/resumo");
            response.EnsureSuccessStatusCode();

            var resumo = await response.Content.ReadFromJsonAsync<List<ContaResumoDto>>();

            Assert.IsNotNull(resumo);
            Assert.IsTrue(resumo!.Exists(c => c.Codigo == ativa.Id));
            Assert.IsFalse(resumo.Exists(c => c.Codigo == inativa.Id));
            Assert.IsFalse(resumo.Exists(c => c.Codigo == contaAlheia.Id));
        }
    }
}
