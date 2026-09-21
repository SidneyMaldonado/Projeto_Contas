namespace Contas_App.Services;

/// <summary>
/// Endereço da API usado por todos os serviços do app.
/// O emulador Android não enxerga "localhost" do host — chega nele pelo IP 10.0.2.2.
/// </summary>
public static class ApiConfig
{
#if ANDROID
    public const string BaseUrl = "http://10.0.2.2:5210/";
#else
    public const string BaseUrl = "http://localhost:5210/";
#endif
}
