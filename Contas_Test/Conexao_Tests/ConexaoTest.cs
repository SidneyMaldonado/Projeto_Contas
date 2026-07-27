using Contas_Db.Model;

namespace Contas_Test.Conexao_Tests
{
    [TestClass]
    public sealed class ConexaoTest
    {
        [TestMethod]
        public async Task DeveConectarAoBanco()
        {
            using var context = new ContasDbContext();

            var conseguiuConectar = await context.Database.CanConnectAsync();

            Assert.IsTrue(conseguiuConectar, "Não foi possível conectar ao banco test_fin em MS211,1434.");
        }
    }
}
