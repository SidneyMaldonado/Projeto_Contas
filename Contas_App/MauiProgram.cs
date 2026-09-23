using Contas_App.Pages;
using Contas_App.Services;
using Microsoft.Extensions.Logging;
using Plugin.Fingerprint;
using Plugin.Fingerprint.Abstractions;

namespace Contas_App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<IFingerprint>(_ => CrossFingerprint.Current);
            builder.Services.AddSingleton<AuthApiService>();

            // AppSession guarda o token do login; ApiClient o anexa em cada requisicao e os
            // servicos por entidade abaixo so mapeiam as rotas - mesma divisao do Contas_Web.
            builder.Services.AddSingleton<AppSession>();
            builder.Services.AddSingleton<ApiClient>();
            builder.Services.AddSingleton<ContasApiService>();
            builder.Services.AddSingleton<ParcelasApiService>();
            builder.Services.AddSingleton<DividasApiService>();
            builder.Services.AddSingleton<ResumoMensalService>();

            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<ContasPage>();

            return builder.Build();
        }
    }
}
