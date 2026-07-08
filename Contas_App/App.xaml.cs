using Microsoft.Extensions.DependencyInjection;

namespace Contas_App
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());

            // Simula a resolução lógica de um celular (ex.: iPhone 13/14) ao rodar no Windows.
            window.Width = 390;
            window.Height = 844;

            return window;
        }
    }
}