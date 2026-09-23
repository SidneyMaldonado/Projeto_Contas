using Contas_Web.Components;
using Contas_Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<AuthSession>();

var apiBaseUrl = builder.Configuration["Api:BaseUrl"] ?? "http://localhost:5210/";

builder.Services.AddHttpClient<AuthApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddHttpClient<DashboardApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// ApiClient concentra o HttpClient e o Bearer token; os serviços por entidade
// abaixo só mapeiam as rotas de cada recurso.
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped<CarteirasApiService>();
builder.Services.AddScoped<CategoriasApiService>();
builder.Services.AddScoped<ContasApiService>();
builder.Services.AddScoped<CredoresApiService>();
builder.Services.AddScoped<DividasApiService>();
builder.Services.AddScoped<HistoricosApiService>();
builder.Services.AddScoped<InvestimentosApiService>();
builder.Services.AddScoped<OperacoesApiService>();
builder.Services.AddScoped<ParcelasApiService>();
builder.Services.AddScoped<UsuariosApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
