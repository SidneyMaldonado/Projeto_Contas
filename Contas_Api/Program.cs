using System.Text;
using Contas_Core.Security;
using Contas_Core.UseCase.Categoria;
using Contas_Core.UseCase.Conta;
using Contas_Core.UseCase.Credor;
using Contas_Core.UseCase.Divida;
using Contas_Core.UseCase.Parcela;
using Contas_Core.UseCase.Usuario;
using Contas_Db.Model;
using Contas_Db.Repository;
using Contas_Db.Repository.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<ContasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<JwtTokenGenerator>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IContaRepository, ContaRepository>();
builder.Services.AddScoped<IParcelaRepository, ParcelaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddScoped<AdicionarCategoriaUseCase>();
builder.Services.AddScoped<AtualizarCategoriaUseCase>();
builder.Services.AddScoped<ExcluirCategoriaUseCase>();
builder.Services.AddScoped<InativarCategoriaUseCase>();
builder.Services.AddScoped<ObterPorIdCategoriaUseCase>();
builder.Services.AddScoped<ObterTodosCategoriaUseCase>();

builder.Services.AddScoped<AdicionarContaUseCase>();
builder.Services.AddScoped<AtualizarContaUseCase>();
builder.Services.AddScoped<AtualizarSaldosContaUseCase>();
builder.Services.AddScoped<ExcluirContaUseCase>();
builder.Services.AddScoped<InativarContaUseCase>();
builder.Services.AddScoped<ObterPorIdContaUseCase>();
builder.Services.AddScoped<ObterResumoContaUseCase>();
builder.Services.AddScoped<ObterTodosContaUseCase>();

builder.Services.AddScoped<AdicionarCredorUseCase>();
builder.Services.AddScoped<AtualizarCredorUseCase>();
builder.Services.AddScoped<ExcluirCredorUseCase>();
builder.Services.AddScoped<InativarCredorUseCase>();
builder.Services.AddScoped<ObterPorIdCredorUseCase>();
builder.Services.AddScoped<ObterTodosCredorUseCase>();

builder.Services.AddScoped<AdicionarDividaUseCase>();
builder.Services.AddScoped<AtualizarDividaUseCase>();
builder.Services.AddScoped<ExcluirDividaUseCase>();
builder.Services.AddScoped<InativarDividaUseCase>();
builder.Services.AddScoped<ObterPorIdDividaUseCase>();
builder.Services.AddScoped<ObterTodosDividaUseCase>();

builder.Services.AddScoped<AdicionarParcelaUseCase>();
builder.Services.AddScoped<AtualizarParcelaUseCase>();
builder.Services.AddScoped<DesfazerPagamentoParcelaUseCase>();
builder.Services.AddScoped<ExcluirParcelaUseCase>();
builder.Services.AddScoped<InativarParcelaUseCase>();
builder.Services.AddScoped<ObterPorIdParcelaUseCase>();
builder.Services.AddScoped<ObterTodosParcelaUseCase>();
builder.Services.AddScoped<PagarParcelaUseCase>();

builder.Services.AddScoped<AdicionarUsuarioUseCase>();
builder.Services.AddScoped<AlterarSenhaUsuarioUseCase>();
builder.Services.AddScoped<AtualizarUsuarioUseCase>();
builder.Services.AddScoped<ExcluirUsuarioUseCase>();
builder.Services.AddScoped<InativarUsuarioUseCase>();
builder.Services.AddScoped<LoginUsuarioUseCase>();
builder.Services.AddScoped<ObterPorIdUsuarioUseCase>();
builder.Services.AddScoped<ObterTodosUsuarioUseCase>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Contas_Api v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
