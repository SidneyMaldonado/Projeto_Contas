using System.Text.RegularExpressions;
using Contas_App.Services;
using Plugin.Fingerprint.Abstractions;

namespace Contas_App.Pages;

public partial class RegisterPage : ContentPage
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled);

    private static readonly Regex SenhaRegex = new(
        @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
        RegexOptions.Compiled);

    private readonly AuthApiService _authApi;
    private readonly IFingerprint _fingerprint;

    public RegisterPage(AuthApiService authApi, IFingerprint fingerprint)
    {
        InitializeComponent();
        _authApi = authApi;
        _fingerprint = fingerprint;
    }

    private async void OnEntrarTapped(object? sender, TappedEventArgs e)
        => await Shell.Current.GoToAsync("//LoginPage");

    private async void OnCadastrarClicked(object? sender, EventArgs e)
    {
        NomeErrorLabel.IsVisible = false;
        EmailErrorLabel.IsVisible = false;
        SenhaErrorLabel.IsVisible = false;
        ConfirmarSenhaErrorLabel.IsVisible = false;
        StatusLabel.IsVisible = false;

        var nome = NomeEntry.Text?.Trim() ?? string.Empty;
        var email = EmailEntry.Text?.Trim() ?? string.Empty;
        var senha = SenhaEntry.Text ?? string.Empty;
        var confirmarSenha = ConfirmarSenhaEntry.Text ?? string.Empty;

        var valid = true;

        if (nome.Length < 3)
        {
            NomeErrorLabel.Text = "Informe um nome com pelo menos 3 caracteres.";
            NomeErrorLabel.IsVisible = true;
            valid = false;
        }

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email))
        {
            EmailErrorLabel.Text = "Informe um e-mail válido.";
            EmailErrorLabel.IsVisible = true;
            valid = false;
        }

        if (!SenhaRegex.IsMatch(senha))
        {
            SenhaErrorLabel.Text = "A senha deve ter ao menos 8 caracteres, com letra maiúscula, minúscula e número.";
            SenhaErrorLabel.IsVisible = true;
            valid = false;
        }

        if (confirmarSenha != senha)
        {
            ConfirmarSenhaErrorLabel.Text = "As senhas não coincidem.";
            ConfirmarSenhaErrorLabel.IsVisible = true;
            valid = false;
        }

        if (!valid)
            return;

        SetBusy(true);
        var registerResult = await _authApi.RegisterAsync(nome, email, senha);

        if (!registerResult.Success)
        {
            SetBusy(false);
            StatusLabel.Text = registerResult.ErrorMessage;
            StatusLabel.IsVisible = true;
            return;
        }

        var loginResult = await _authApi.LoginAsync(email, senha);
        SetBusy(false);

        if (!loginResult.Success)
        {
            StatusLabel.Text = "Cadastro realizado! Faça login para continuar.";
            StatusLabel.TextColor = Colors.White;
            StatusLabel.IsVisible = true;
            await Shell.Current.GoToAsync("//LoginPage");
            return;
        }

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
        CadastrarButton.IsEnabled = !busy;
    }
}
