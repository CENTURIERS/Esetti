using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Input;

namespace Esseti.ViewModels.Components
{
    /// <summary>
    /// Model widoku reprezentujący pojedynczą pozycję w menu nawigacyjnym (np. "Członkowie", "Projekty", "Ustawienia").
    /// Przechowuje ikonę, tekst przycisku, flagę aktywności oraz komendę do wykonania po kliknięciu.
    /// </summary>
    public partial class NavigationItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Nazwa ikony (np. klucz tekstowy do ikon wektorowych).
        /// </summary>
        [ObservableProperty]
        private string? _icon;

        /// <summary>
        /// Tekstowa etykieta wyświetlana obok ikony w menu.
        /// </summary>
        [ObservableProperty]
        private string? _label;

        /// <summary>
        /// Czy ta pozycja menu jest w tym momencie aktywna (np. podświetlona, bo użytkownik na niej przebywa).
        /// </summary>
        [ObservableProperty]
        private bool _isActive;

        /// <summary>
        /// Komenda wywoływana przy kliknięciu w tę pozycję nawigacji (najczęściej przełączająca główny widok).
        /// </summary>
        public ICommand? Command { get; }

        /// <summary>
        /// Konstruktor pozycji menu nawigacyjnego.
        /// </summary>
        /// <param name="icon">Nazwa/kod ikony.</param>
        /// <param name="label">Napis wyświetlany w menu.</param>
        /// <param name="command">Komenda do wykonania przy kliknięciu.</param>
        public NavigationItemViewModel(string icon, string label, ICommand command)
        {
            Icon = icon;
            Label = label;
            Command = command;
        }
    }
}
