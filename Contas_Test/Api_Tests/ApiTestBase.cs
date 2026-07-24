using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using Contas_Core.Security;
using Contas_Db.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Contas_Test.Api_Tests
{
    public abstract class ApiTestBase
    {
        private class ApiFactory : WebApplicationFactory<Program>
        {
            private readonly string _databaseName = Guid.NewGuid().ToString();

            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                builder.ConfigureServices(services =>
                {
                    var descriptors = services
                        .Where(d => d.ServiceType == typeof(ContasDbContext)
                                 || (d.ServiceType.IsGenericType
                                     && d.ServiceType.GetGenericArguments().Contains(typeof(ContasDbContext))))
                        .ToList();

                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<ContasDbContext>(options =>
                        options.UseInMemoryDatabase(_databaseName));
                });
            }
        }

        private ApiFactory _factory = null!;

        /// <summary>Cliente autenticado como <see cref="CurrentUser"/> (token Bearer já anexado).</summary>
        protected HttpClient Client { get; private set; } = null!;

        /// <summary>Usuário seedado e autenticado automaticamente para cada teste.</summary>
        protected Usuario CurrentUser { get; private set; } = null!;

        [TestInitialize]
        public async Task ApiTestBaseSetup()
        {
            _factory = new ApiFactory();

            CurrentUser = await SeedAsync(new Usuario
            {
                Nome = "Usuário de Teste",
                Email = $"{Guid.NewGuid()}@teste.com",
                Senha = PasswordHasher.Hash("Senha123"),
                Ativo = true
            });

            Client = _factory.CreateClient();
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateToken(CurrentUser));
        }

        [TestCleanup]
        public void ApiTestBaseCleanup()
        {
            Client.Dispose();
            _factory.Dispose();
        }

        /// <summary>Cliente sem token, para testar cenários de 401 Unauthorized.</summary>
        protected HttpClient CreateAnonymousClient() => _factory.CreateClient();

        protected string GenerateToken(Usuario usuario)
        {
            using var scope = _factory.Services.CreateScope();
            var generator = scope.ServiceProvider.GetRequiredService<JwtTokenGenerator>();
            return generator.GenerateToken(usuario.Id, usuario.Email);
        }

        protected async Task<T> SeedAsync<T>(T entity) where T : class
        {
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ContasDbContext>();

            context.Set<T>().Add(entity);
            await context.SaveChangesAsync();

            return entity;
        }
    }
}
