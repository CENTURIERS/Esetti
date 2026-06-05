using Avalonia;
using Avalonia.ReactiveUI;
using System;

namespace Esseti
{
    /// <summary>
    /// Główny punkt wejścia do naszej aplikacji (punkt startowy).
    /// </summary>
    internal sealed class Program
    {
        /// <summary>
        /// Główna funkcja uruchomieniowa (Main). Odpala aplikację w klasycznym trybie desktopowym.
        /// Używa atrybutu STAThread, który jest wymagany przez system Windows do poprawnego działania UI.
        /// </summary>
        /// <param name="args">Argumenty przekazane z linii komend przy uruchamianiu exe.</param>
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        /// <summary>
        /// Konfiguruje Avalonię – ustawia platformę (Windows/Linux/macOS), czcionki InterFont,
        /// logowanie diagnostyczne oraz włącza wsparcie dla ReactiveUI.
        /// </summary>
        /// <returns>Obiekt konfiguracji AppBuilder.</returns>
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}


