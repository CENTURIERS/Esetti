using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Esseti.ViewModels;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Esseti
{
    
    
    
    /// <summary>
    /// Klasa odpowiedzialna za mapowanie (szukanie) widoku (View) dla odpowiadającego mu modelu widoku (ViewModel).
    /// Działa automatycznie za pomocą refleksji, podmieniając słowo "ViewModel" w nazwie klasy na "View".
    /// </summary>
    [RequiresUnreferencedCode(
        "Default implementation of ViewLocator involves reflection which may be trimmed away.",
        Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
    public class ViewLocator : IDataTemplate
    {
        /// <summary>
        /// Tworzy instancję odpowiedniej kontrolki (View) na podstawie przekazanego obiektu ViewModel.
        /// Jeśli widoku nie uda się znaleźć, zwraca prosty TextBlock z informacją o błędzie.
        /// </summary>
        /// <param name="param">Obiekt typu ViewModel, dla którego szukamy widoku.</param>
        /// <returns>Utworzony widok (kontrolka Avalonia) lub TextBlock w razie błędu.</returns>
        public Control? Build(object? param)
        {
            if (param is null)
                return null;

            var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }

            return new TextBlock { Text = "Not Found: " + name };
        }

        /// <summary>
        /// Sprawdza, czy dany obiekt kwalifikuje się do dopasowania widoku.
        /// Interesują nas tylko te klasy, które dziedziczą po <see cref="ViewModelBase"/>.
        /// </summary>
        /// <param name="data">Obiekt danych do sprawdzenia.</param>
        /// <returns>True, jeśli obiekt dziedziczy po ViewModelBase.</returns>
        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}


