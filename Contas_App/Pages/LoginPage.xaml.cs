using System.Text.RegularExpressions;
using Contas_App.Services;
using Plugin.Fingerprint.Abstractions;

namespace Contas_App.Pages;

public partial class LoginPage : ContentPage
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    private readonly AuthApiService _authApi;
    private readonly IFingerprint _fingerprint;

    public LoginPage(AuthApiService authApi, IFingerprint fingerprint)
    {
        InitializeComponent();
        _authApi = authApi;
        _fingerprint = fingerprint;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        StatusLabel.IsVisible = false;

        if (!CredentialStore.IsEnabled)
        {
            BiometriaButton.IsVisible = false;
            return;
        }

        var availability = await _fingerprint.GetAvailabilityAsync();
        BiometriaButton.IsVisible = availability == FingerprintAvailability.Available;

        if (availability == FingerprintAvailability.Available)
            await TryBiometricLoginAsync();
    }

    private async void OnBiometriaClicked(object? sender, EventArgs e)
        => await TryBiometricLoginAsync();

    private async Task TryBiometricLoginAsync()
    {
        var config = new AuthenticationRequestConfiguration("Vida dura", "Autentique-se para acessar sua conta")
        {
            CancelTitle = "Cancelar",
            FallbackTitle = "Usar senha"
        };

        var result = await _fingerprint.AuthenticateAsync(config);
        if (!result.Authenticated)
            return;

        var credentials = await CredentialStore.LoadAsync();
        if (credentials is null)
        {
            CredentialStore.Disable();
            return;
        }

        await DoLoginAsync(credentials.Value.Email, credentials.Value.Senha, offerBiometricSetup: false);
    }

    private async void OnAcessarClicked(object? sender, EventArgs e)
    {
        EmailErrorLabel.IsVisible = false;
        SenhaErrorLabel.IsVisible = false;
        StatusLabel.IsVisible = false;

        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var senha = SenhaEntry.Text ?? string.Empty;

        var valid = true;

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
        {
            EmailErrorLabel.Text = "Informe um e-mail válido.";
            EmailErrorLabel.IsVisible = true;
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(senha))
        {
            SenhaErrorLabel.Text = "A senha não pode ser vazia.";
            SenhaErrorLabel.IsVisible = true;
            valid = false;
        }

        if (!valid)
            return;

        await DoLoginAsync(email, senha, offerBiometricSetup: true);
    }

    private async Task DoLoginAsync(string email, string senha, bool offerBiometricSetup)
    {
        SetBusy(true);
        var result = await _authApi.LoginAsync(email, senha);
        SetBusy(false);

        if (!result.Success)
        {
            StatusLabel.Text = result.ErrorMessage;
            StatusLabel.IsVisible = true;
            return;
        }

        if (offerBiometricSetup && !CredentialStore.IsEnabled)
            await OfferBiometricSetupAsync(email, senha);

        await Shell.Current.GoToAsync("//MainPage");
    }

    private async Task OfferBiometricSetupAsync(string email, string senha)
    {
        var availability = await _fingerprint.GetAvailabilityAsync();
        if (availability != FingerprintAvailability.Available)
            return;

        var quer = await this.DisplayAlertAsync(
            "Biometria",
            "Deseja usar a biometria para entrar da próxima vez?",
            "Sim", "Não");

        if (!quer)
            return;

        var config = new AuthenticationRequestConfiguration("Vida dura", "Confirme sua biometria para ativar o login rápido");
        var result = await _fingerprint.AuthenticateAsync(config);

        if (result.Authenticated)
            await CredentialStore.SaveAsync(email, senha);
    }

    private void SetBusy(bool busy)
    {
        LoadingIndicator.IsVisible = busy;
        LoadingIndicator.IsRunning = busy;
        AcessarButton.IsEnabled = !busy;
    }

    private async void OnRegistrarSeTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//RegisterPage");

    private async void OnEsqueciSenhaTapped(object? sender, TappedEventArgs e)
        => await this.DisplayAlertAsync("Esqueci minha senha", "Essa funcionalidade ainda não está disponível.", "OK");
}
