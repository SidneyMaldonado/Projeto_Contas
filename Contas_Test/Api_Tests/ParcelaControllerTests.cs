using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Contas_Contratos.Dto;
using Contas_Db.Model;

namespace Contas_Test.Api_Tests
{
    [TestClass]
    public class ParcelaControllerTests : ApiTestBase
    {
        private Task<Usuario> SeedOutroUsuarioAsync() =>
            SeedAsync(new Usuario { Nome = "Outro UsuÃ¡rio", Email = $"{Guid.NewGuid()}@teste.com", Senha = "hash", Ativo = true });

        private Task<Categoria> SeedCategoriaAsync() =>
            SeedAsync(new Categoria { Nome = "AlimentaÃ§Ã£o", Ativo = true });

        private Task<Conta> SeedContaAsync(int idUsuario) =>
            SeedAsync(new Conta { IdUsuario = idUsuario, Nome = "Conta Corrente", Saldo = 1000m, Ativo = true });

        private Task<Divida> SeedDividaAsync(int idUsuario)
        {
            var dataVencimento = DateTime.Today.AddMonths(1);

            return SeedAsync(new Divida
            {
                IdUsuario = idUsuario,
                Nome = "Financiamento",
                DiaVencimento = dataVencimento.Day,
                DataPrimeiroVencimento = dataVencimento,
                Parcelas = 12,
                Valor = 1200m,
                Ativo = true
            });
        }

        private async Task<(Divida Divida, Categoria Categoria, Conta Conta)> SeedDependenciasAsync(int idUsuario)
        {
            var categoria = await SeedCategoriaAsync();
            var conta = await SeedContaAsync(idUsuario);
            var divida = await SeedDividaAsync(idUsuario);

            return (divida, categoria, conta);
        }

        private async Task<Parcela> SeedParcelaAsync(
            int idDivida, int idCategoria, int idConta,
            string descricao = "Parcela 1/12", decimal valor = 100m, bool ativo = true)
        {
            var parcela = new Parcela
            {
                IdDivida = idDivida,
                IdCategoria = idCategoria,
                IdConta = idConta,
                Descricao = descricao,
                Valor = valor,
                DataVencimento = DateTime.Today.AddMonths(1),
                Ativo = ativo
            };

            return await SeedAsync(parcela);
        }

        [TestMethod]
        public async Task ObterTodos_DeveRetornarListaDeParcelas()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);
            await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);

            var response = await Client.GetAsync("/api/parcelas");
            response.EnsureSuccessStatusCode();

            var parcelas = await response.Content.ReadFromJsonAsync<List<ParcelaDto>>();

            Assert.IsNotNull(parcelas);
            Assert.IsNotEmpty(parcelas);
        }

        [TestMethod]
        public async Task ObterTodos_NaoDeveRetornarParcelaDeOutroUsuario()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);
            await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id, "Minha Parcela");

            var outroUsuario = await SeedOutroUsuarioAsync();
            var (dividaAlheia, categoriaAlheia, contaAlheia) = await SeedDependenciasAsync(outroUsuario.Id);
            var parcelaAlheia = await SeedParcelaAsync(dividaAlheia.Id, categoriaAlheia.Id, contaAlheia.Id, "Parcela Alheia");

            var response = await Client.GetAsync("/api/parcelas");
            var parcelas = await response.Content.ReadFromJsonAsync<List<ParcelaDto>>();

            Assert.IsFalse(parcelas!.Exists(p => p.Id == parcelaAlheia.Id));
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarParcela_QuandoExistir()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);
            var parcela = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id, "Parcela 2/12", 200m);

            var response = await Client.GetAsync($"/api/parcelas/{parcela.Id}");
            response.EnsureSuccessStatusCode();

            var dto = await response.Content.ReadFromJsonAsync<ParcelaDto>();

            Assert.IsNotNull(dto);
            Assert.AreEqual(parcela.Id, dto!.Id);
            Assert.AreEqual("Parcela 2/12", dto.Descricao);
            Assert.AreEqual(200m, dto.Valor);
            Assert.IsFalse(dto.Pago);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.GetAsync("/api/parcelas/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task ObterPorId_DeveRetornarNotFound_QuandoParcelaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (divida, categoria, conta) = await SeedDependenciasAsync(outroUsuario.Id);
            var parcelaAlheia = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);

            var response = await Client.GetAsync($"/api/parcelas/{parcelaAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveCriarParcela_QuandoValida()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AdicionarParcelaDto
            {
                IdDivida = divida.Id,
                IdCategoria = categoria.Id,
                IdConta = conta.Id,
                Descricao = "Parcela 3/12",
                Valor = 100m,
                DataVencimento = DateTime.Today.AddMonths(1)
            };

            var response = await Client.PostAsJsonAsync("/api/parcelas", dto);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var criada = await response.Content.ReadFromJsonAsync<ParcelaDto>();
            Assert.IsNotNull(criada);
            Assert.AreEqual("Parcela 3/12", criada!.Descricao);
            Assert.AreNotEqual(0, criada.Id);
            Assert.IsTrue(criada.Ativo);
            Assert.IsFalse(criada.Pago);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarNotFound_QuandoContaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (dividaAlheia, categoria, contaAlheia) = await SeedDependenciasAsync(outroUsuario.Id);

            var dto = new AdicionarParcelaDto
            {
                IdDivida = dividaAlheia.Id,
                IdCategoria = categoria.Id,
                IdConta = contaAlheia.Id,
                Descricao = "Tentativa de InvasÃ£o",
                Valor = 100m,
                DataVencimento = DateTime.Today.AddMonths(1)
            };

            var response = await Client.PostAsJsonAsync("/api/parcelas", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Adicionar_DeveRetornarBadRequest_QuandoValorInvalido()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AdicionarParcelaDto
            {
                IdDivida = divida.Id,
                IdCategoria = categoria.Id,
                IdConta = conta.Id,
                Descricao = "Parcela InvÃ¡lida",
                Valor = 0m,
                DataVencimento = DateTime.Today.AddMonths(1)
            };

            var response = await Client.PostAsJsonAsync("/api/parcelas", dto);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveAtualizarParcela_QuandoExistir()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);
            var parcela = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id, "Nome Antigo", 100m);

            var dto = new AtualizarParcelaDto
            {
                IdDivida = divida.Id,
                IdCategoria = categoria.Id,
                IdConta = conta.Id,
                Descricao = "Nome Novo",
                Valor = 350m,
                DataVencimento = DateTime.Today.AddMonths(2)
            };

            var response = await Client.PutAsJsonAsync($"/api/parcelas/{parcela.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/parcelas/{parcela.Id}");
            var atualizada = await consulta.Content.ReadFromJsonAsync<ParcelaDto>();
            Assert.AreEqual("Nome Novo", atualizada!.Descricao);
            Assert.AreEqual(350m, atualizada.Valor);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);

            var dto = new AtualizarParcelaDto
            {
                IdDivida = divida.Id,
                IdCategoria = categoria.Id,
                IdConta = conta.Id,
                Descricao = "Qualquer",
                Valor = 100m,
                DataVencimento = DateTime.Today.AddMonths(1)
            };

            var response = await Client.PutAsJsonAsync("/api/parcelas/999999", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Atualizar_DeveRetornarNotFound_QuandoParcelaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (divida, categoria, conta) = await SeedDependenciasAsync(outroUsuario.Id);
            var parcelaAlheia = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);

            var dto = new AtualizarParcelaDto
            {
                IdDivida = divida.Id,
                IdCategoria = categoria.Id,
                IdConta = conta.Id,
                Descricao = "InvasÃ£o",
                Valor = 100m,
                DataVencimento = DateTime.Today.AddMonths(1)
            };

            var response = await Client.PutAsJsonAsync($"/api/parcelas/{parcelaAlheia.Id}", dto);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Pagar_DeveMarcarParcelaComoPaga_QuandoExistir()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);
            var parcela = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);
            var dataPagamento = DateTime.Today;

            var response = await Client.PatchAsJsonAsync($"/api/parcelas/{parcela.Id}/pagar", new PagarParcelaDto { DataPagamento = dataPagamento });
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/parcelas/{parcela.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<ParcelaDto>();

            Assert.IsTrue(dto!.Pago);
            Assert.AreEqual(dataPagamento, dto.DataPagamento);
        }

        [TestMethod]
        public async Task Pagar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsJsonAsync("/api/parcelas/999999/pagar", new PagarParcelaDto { DataPagamento = DateTime.Today });

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Pagar_DeveRetornarNotFound_QuandoParcelaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (divida, categoria, conta) = await SeedDependenciasAsync(outroUsuario.Id);
            var parcelaAlheia = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);

            var response = await Client.PatchAsJsonAsync($"/api/parcelas/{parcelaAlheia.Id}/pagar", new PagarParcelaDto { DataPagamento = DateTime.Today });

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DesfazerPagamento_DeveDesfazerPagamento_QuandoExistir()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);
            var parcela = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);

            await Client.PatchAsJsonAsync($"/api/parcelas/{parcela.Id}/pagar", new PagarParcelaDto { DataPagamento = DateTime.Today });

            var response = await Client.PatchAsync($"/api/parcelas/{parcela.Id}/desfazer-pagamento", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/parcelas/{parcela.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<ParcelaDto>();

            Assert.IsFalse(dto!.Pago);
            Assert.IsNull(dto.DataPagamento);
        }

        [TestMethod]
        public async Task DesfazerPagamento_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/parcelas/999999/desfazer-pagamento", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task DesfazerPagamento_DeveRetornarNotFound_QuandoParcelaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (divida, categoria, conta) = await SeedDependenciasAsync(outroUsuario.Id);
            var parcelaAlheia = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);

            var response = await Client.PatchAsync($"/api/parcelas/{parcelaAlheia.Id}/desfazer-pagamento", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRemoverParcela_QuandoExistir()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);
            var parcela = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);

            var response = await Client.DeleteAsync($"/api/parcelas/{parcela.Id}");
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/parcelas/{parcela.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, consulta.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.DeleteAsync("/api/parcelas/999999");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Excluir_DeveRetornarNotFound_QuandoParcelaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (divida, categoria, conta) = await SeedDependenciasAsync(outroUsuario.Id);
            var parcelaAlheia = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);

            var response = await Client.DeleteAsync($"/api/parcelas/{parcelaAlheia.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveInativarParcela_QuandoExistir()
        {
            var (divida, categoria, conta) = await SeedDependenciasAsync(CurrentUser.Id);
            var parcela = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id, ativo: true);

            var response = await Client.PatchAsync($"/api/parcelas/{parcela.Id}/inativar", null);
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            var consulta = await Client.GetAsync($"/api/parcelas/{parcela.Id}");
            var dto = await consulta.Content.ReadFromJsonAsync<ParcelaDto>();
            Assert.IsFalse(dto!.Ativo);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoNaoExistir()
        {
            var response = await Client.PatchAsync("/api/parcelas/999999/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Inativar_DeveRetornarNotFound_QuandoParcelaNaoPertenceAoUsuarioAtual()
        {
            var outroUsuario = await SeedOutroUsuarioAsync();
            var (divida, categoria, conta) = await SeedDependenciasAsync(outroUsuario.Id);
            var parcelaAlheia = await SeedParcelaAsync(divida.Id, categoria.Id, conta.Id);

            var response = await Client.PatchAsync($"/api/parcelas/{parcelaAlheia.Id}/inativar", null);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
