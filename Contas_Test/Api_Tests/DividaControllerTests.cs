using System.Collections.Generic;
using System.Linq;
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
            SeedAsync(new Usuario { Nome = "Outro Usuário", Email = $"{Guid.NewGuid()}@teste.com", Senha = "hash", Ativo = true });

        private Task<Categoria> SeedCategoriaAsync() =>
            SeedAsync(new Categoria { Nome = "Financiamentos", Ativo = true });

        private Task<Conta> SeedContaAsync(int idUsuario) =>
            SeedAsync(new Conta { IdUsuario = idUsuario, Nome = "Conta Corrente", Saldo = 1000m, Ativo = true });

        private async Task<Divida> SeedDividaAsync(int idUsuario, string nome = "Financiamento", decimal valor = 1000m, bool ativo = true)
        {
            var categoria = await SeedCategoriaAsync();
            var conta = await SeedContaAsync(idUsuario);
            var dataVencimento = DateTime.Today.AddMonths(1);

            return await SeedAsync(new Divida
            {
                IdUsuario = idUsuario,
                IdConta = conta.Id,
                IdCategoria = categoria.Id,
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
            await SeedDividaAsync(CurrentUser.Id, "Minha Dívida");
            var outroUsuario = await SeedOutroUsuarioAsync();
            var dividaAlheia = await SeedDividaAsync(outroUsuario.Id, "Dívida Alheia");

            var response = await Client.GetAsync("/api/dividas");
            var dividas = await response.Content.ReadFromJsonAsync<List<DividaDto>>();

            Assert.IsFalse(dividas!.Exists(d => d.Id == dividaAlheia.Id));
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarDivida_QuandoExistir()
        {
            var divida = await SeedDividaAsync(CurrentUser.Id, "Cartão de Crédito", 2000m);

            var response = await Client.GetAsync($"/api/dividas/{divida.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<DividaDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(divida.Id, dto!.Id);
            Assert.AreEqual("Cartão de Crédito", dto.Nome);
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
            var categoria = await SeedCategoriaAsync();
            var conta = await SeedContaAsync(CurrentUser.Id);
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                IdConta = conta.Id,
                IdCategoria = categoria.Id,
                Nome = "Empréstimo Pessoal",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 6,
                Valor = 500m
            };

            var response = await Client.PostAsJsonAsync("/api/dividas", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<DividaDto>();
            Assert.IsNotNull(criada);
            Assert.AreEqual("Empréstimo Pessoal", criada!.Nome);
            Assert.AreNotEqual(0, criada.Id);
            Assert.IsTrue(criada.Ativo);
        }

        [TestMethod]
        public async Task Adicionar_DeveGerarParcelas_ComValorDivididoEDataIncrementadaPorMes()
        {
            var categoria = await SeedCategoriaAsync();
            var conta = await SeedContaAsync(CurrentUser.Id);
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                IdConta = conta.Id,
                IdCategoria = categoria.Id,
                Nome = "Financiamento Veículo",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 3,
                Valor = 100m
            };

            var response = await Client.PostAsJsonAsync("/api/dividas", dto);
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var criada = await response.Content.ReadFromJsonAsync<DividaDto>();

            var parcelasResponse = await Client.GetAsync("/api/parcelas");
            var parcelas = await parcelasResponse.Content.ReadFromJsonAsync<List<ParcelaDto>>();
            var geradas = parcelas!.Where(p => p.IdDivida == criada!.Id).OrderBy(p => p.DataVencimento).ToList();

            Assert.HasCount(3, geradas);
            Assert.AreEqual(dataVencimento.Date, geradas[0].DataVencimento.Date);
            Assert.AreEqual(dataVencimento.AddMonths(1).Date, geradas[1].DataVencimento.Date);
            Assert.AreEqual(dataVencimento.AddMonths(2).Date, geradas[2].DataVencimento.Date);
            Assert.IsTrue(geradas.All(p => p.IdConta == conta.Id && p.IdCategoria == categoria.Id));
            Assert.AreEqual(100m, geradas.Sum(p => p.Valor));
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarNotFound_QuandoContaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var contaAlheia = await SeedContaAsync(outroUsuario.Id);
            var categoria = await SeedCategoriaAsync();
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                IdConta = contaAlheia.Id,
                IdCategoria = categoria.Id,
                Nome = "Invasão",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 1,
                Valor = 100m
            };

            var response = await Client.PostAsJsonAsync("/api/dividas", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveIgnorarIdUsuarioDoDto_EUsarUsuarioAutenticado()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var categoria = await SeedCategoriaAsync();
            var conta = await SeedContaAsync(CurrentUser.Id);
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = outroUsuario.Id,
                IdConta = conta.Id,
                IdCategoria = categoria.Id,
                Nome = "Dívida Forjada",
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
            var categoria = await SeedCategoriaAsync();
            var conta = await SeedContaAsync(CurrentUser.Id);
            var dataVencimento = DateTime.Today.AddMonths(1);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                IdConta = conta.Id,
                IdCategoria = categoria.Id,
                Nome = "Dívida Inválida",
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
            var categoria = await SeedCategoriaAsync();
            var conta = await SeedContaAsync(CurrentUser.Id);
            var dataVencimento = DateTime.Today.AddDays(-5);

            var dto = new AdicionarDividaDto
            {
                IdUsuario = CurrentUser.Id,
                IdConta = conta.Id,
                IdCategoria = categoria.Id,
                Nome = "Dívida Vencida",
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
                Nome = "Invasão",
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
