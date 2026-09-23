using System.Globalization;
using GraphicsFont = Microsoft.Maui.Graphics.Font;

#if WINDOWS
using Microsoft.Maui.Graphics.Win2D;
#else
using Microsoft.Maui.Graphics.Platform;
#endif

namespace Contas_App.Services;

/// <summary>
/// Desenha o quadro de saldos como PNG para compartilhar em outros apps. Fundo branco,
/// texto preto e grade fechada: o app é escuro, mas a imagem vai para conversas onde o
/// tema claro é o que se lê bem — e em tabela, como a planilha que isso substitui.
/// </summary>
public static class SaldosImagemService
{
    public const string Titulo = "Vida Dura";

    private static readonly CultureInfo Moeda = new("pt-BR");

    private static readonly Color Grade = Color.FromArgb("#808080");
    private static readonly Color FundoDestaque = Color.FromArgb("#D9D9D9");

    // Medidas base pensadas em ~500px de largura; a escala sobe tudo junto para o PNG
    // não sair granulado quando o app de destino amplia a imagem.
    private const float Escala = 2f;
    private const float Fonte = 28f * Escala;
    private const float AlturaLinha = 52f * Escala;
    private const float Respiro = 18f * Escala;

    // O Maui.Graphics não expõe medição de texto aqui, então a largura das colunas sai de
    // uma estimativa por caractere folgada: nome comprido alarga a imagem em vez de ser cortado.
    private const float LarguraPorCaractere = Fonte * 0.58f;

    public static byte[] GerarPng(IReadOnlyList<ContaSaldoItem> contas)
    {
        var nomes = contas.Select(c => c.Nome).ToList();
        var valores = contas.Select(c => Formatar(c.Saldo)).ToList();
        var total = Formatar(contas.Sum(c => c.Saldo));
        var data = DateTime.Now.ToString("dd/MMM", Moeda).TrimEnd('.');

        var colunaNome = LarguraColuna([Titulo, "Total", .. nomes], minimo: 240f);
        var colunaValor = LarguraColuna([data, total, .. valores], minimo: 180f);

        var largura = colunaNome + colunaValor;
        var altura = AlturaLinha * (contas.Count + 2);

#if WINDOWS
        var servico = new W2DBitmapExportService();
#else
        var servico = new PlatformBitmapExportService();
#endif
        using var contexto = servico.CreateContext((int)largura, (int)altura);
        var canvas = contexto.Canvas;

        canvas.FillColor = Colors.White;
        canvas.FillRectangle(0, 0, largura, altura);

        DesenharLinha(canvas, 0, colunaNome, colunaValor, Titulo, data, destaque: true);

        for (var i = 0; i < contas.Count; i++)
            DesenharLinha(canvas, (i + 1) * AlturaLinha, colunaNome, colunaValor, nomes[i], valores[i], destaque: false);

        DesenharLinha(canvas, (contas.Count + 1) * AlturaLinha, colunaNome, colunaValor, "Total", total, destaque: true);

        DesenharGrade(canvas, largura, altura, colunaNome, contas.Count + 2);

        using var imagem = contexto.Image;
        using var memoria = new MemoryStream();
        imagem.Save(memoria, ImageFormat.Png);
        return memoria.ToArray();
    }

    private static void DesenharLinha(
        ICanvas canvas, float y, float colunaNome, float colunaValor,
        string nome, string valor, bool destaque)
    {
        if (destaque)
        {
            canvas.FillColor = FundoDestaque;
            canvas.FillRectangle(0, y, colunaNome + colunaValor, AlturaLinha);
        }

        canvas.Font = destaque ? GraphicsFont.DefaultBold : GraphicsFont.Default;
        canvas.FontSize = Fonte;
        canvas.FontColor = Colors.Black;

        canvas.DrawString(nome, Respiro, y, colunaNome - 2 * Respiro, AlturaLinha,
            HorizontalAlignment.Left, VerticalAlignment.Center);

        canvas.DrawString(valor, colunaNome + Respiro, y, colunaValor - 2 * Respiro, AlturaLinha,
            HorizontalAlignment.Right, VerticalAlignment.Center);
    }

    private static void DesenharGrade(ICanvas canvas, float largura, float altura, float colunaNome, int linhas)
    {
        canvas.StrokeColor = Grade;
        canvas.StrokeSize = Escala;

        // Recuo de meio traço na borda externa: o traço é centrado na linha e metade dele
        // cairia fora do bitmap se o retângulo começasse em zero.
        canvas.DrawRectangle(Escala / 2, Escala / 2, largura - Escala, altura - Escala);

        for (var i = 1; i < linhas; i++)
        {
            var y = i * AlturaLinha;
            canvas.DrawLine(0, y, largura, y);
        }

        canvas.DrawLine(colunaNome, 0, colunaNome, altura);
    }

    private static float LarguraColuna(IReadOnlyList<string> textos, float minimo) =>
        Math.Max(minimo, textos.Max(t => t.Length) * LarguraPorCaractere + 2 * Respiro);

    private static string Formatar(decimal valor) => valor.ToString("N2", Moeda);
}
