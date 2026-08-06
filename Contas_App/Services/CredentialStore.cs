namespace Contas_App.Services;

public static class CredentialStore
{
    private const string EmailKey = "biometric_email";
    private const string SenhaKey = "biometric_senha";
    private const string EnabledKey = "biometric_enabled";

    public static bool IsEnabled => Preferences.Default.Get(EnabledKey, false);

    public static async Task SaveAsync(string email, string senha)
    {
        await SecureStorage.Default.SetAsync(EmailKey, email);
        await SecureStorage.Default.SetAsync(SenhaKey, senha);
        Preferences.Default.Set(EnabledKey, true);
    }

    public static async Task<(string Email, string Senha)?> LoadAsync()
    {
        var email = await SecureStorage.Default.GetAsync(EmailKey);
        var senha = await SecureStorage.Default.GetAsync(SenhaKey);

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(senha))
            return null;

        return (email, senha);
    }

    public static void Disable()
    {
        SecureStorage.Default.Remove(EmailKey);
        SecureStorage.Default.Remove(SenhaKey);
        Preferences.Default.Set(EnabledKey, false);
    }
}
