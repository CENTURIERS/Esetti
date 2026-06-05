using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace Esseti.ViewModels
{
    /// <summary>
    /// Klasa bazowa dla wszystkich naszych modeli widoku (ViewModeli).
    /// Dziedziczy po ObservableObject, żeby bindowanie danych i powiadomienia działały bez problemu.
    /// Implementuje też IDisposable do sprzątania po sobie.
    /// </summary>
    public partial class ViewModelBase : ObservableObject, IDisposable
    {
        /// <summary>
        /// Zwalnia zasoby używane przez model widoku.
        /// </summary>
        public virtual void Dispose()
        {
        }

        /// <summary>
        /// Tytuł strony, który wyświetla się na samej górze widoku.
        /// </summary>
        public virtual string PageTitle => "Aplikacja Esseti";

        /// <summary>
        /// Flaga mówiąca, czy mamy pokazywać nagłówek akcji w widoku.
        /// </summary>
        public virtual bool ShowActionHeader => false;

        /// <summary>
        /// Tekst zastępczy (placeholder) dla pola wyszukiwania.
        /// </summary>
        public virtual string SearchPlaceholder => "Szukaj...";

        /// <summary>
        /// Wyszukiwana fraza wpisana przez użytkownika w polu tekstowym.
        /// </summary>
        [ObservableProperty]
        private string _searchQuery = string.Empty;

        /// <summary>
        /// Pomocnicza flaga, żebyśmy się nie zapętlili przy aktualizowaniu zaznaczenia elementów.
        /// </summary>
        protected bool _isUpdatingSelection;

        /// <summary>
        /// Czy wszystkie elementy na liście są zaznaczone.
        /// </summary>
        [ObservableProperty]
        private bool _isAllSelected;

        /// <summary>
        /// Czy jakikolwiek element na liście jest zaznaczony.
        /// </summary>
        [ObservableProperty]
        private bool _isAnySelected;

        /// <summary>
        /// Liczba zaznaczonych elementów na liście.
        /// </summary>
        [ObservableProperty]
        private int _selectedCount;

        /// <summary>
        /// Czy główne okienko popup (np. potwierdzenie usunięcia) ma być widoczne.
        /// </summary>
        [ObservableProperty]
        private bool _isPopupVisible;

        /// <summary>
        /// Czy okienko dodawania nowego elementu ma być otwarte.
        /// </summary>
        [ObservableProperty]
        private bool _isAddPopupVisible;

        /// <summary>
        /// Czy okienko edycji elementu ma być otwarte.
        /// </summary>
        [ObservableProperty]
        private bool _isEditPopupVisible;

        /// <summary>
        /// Automatyczna metoda wywoływana, gdy zmienia się widoczność popupa.
        /// </summary>
        /// <param name="value">Nowa wartość określająca czy popup jest widoczny.</param>
        partial void OnIsPopupVisibleChanged(bool value)
        {
            if (!value) OnPopupClosed();
        }

        /// <summary>
        /// Metoda wywoływana po zamknięciu popupa, do ewentualnego nadpisania w klasach pochodnych.
        /// </summary>
        protected virtual void OnPopupClosed() { }

        /// <summary>
        /// Wywołuje się automatycznie, gdy zmieni się zaznaczenie wszystkich elementów na liście.
        /// </summary>
        /// <param name="value">Nowa wartość zaznaczenia wszystkich elementów.</param>
        partial void OnIsAllSelectedChanged(bool value)
        {
            OnIsAllSelectedChangedVirtual(value);
        }

        /// <summary>
        /// Wirtualna metoda obsługująca zmianę zaznaczenia wszystkich elementów, do nadpisania w podklasach.
        /// </summary>
        /// <param name="value">Wartość określająca czy wszystko jest zaznaczone.</param>
        protected virtual void OnIsAllSelectedChangedVirtual(bool value)
        {
        }

        /// <summary>
        /// Wywołuje się automatycznie, gdy użytkownik wpisze coś w wyszukiwarkę.
        /// </summary>
        /// <param name="value">Nowy tekst wyszukiwania.</param>
        partial void OnSearchQueryChanged(string value)
        {
            OnSearchQueryUpdated(value);
        }

        /// <summary>
        /// Wirtualna metoda do faktycznej obsługi wpisania tekstu w wyszukiwarkę, do nadpisania w podklasach.
        /// </summary>
        /// <param name="value">Wyszukiwany tekst.</param>
        protected virtual void OnSearchQueryUpdated(string value)
        {

        }

        /// <summary>
        /// Komenda do kliknięcia - żądanie usunięcia zaznaczonych elementów. Pokazuje popup z potwierdzeniem.
        /// </summary>
        [RelayCommand]
        protected void RequestDelete()
        {
            if (IsAnySelected)
            {
                IsPopupVisible = true;
            }
        }

        /// <summary>
        /// Komenda do kliknięcia - anulowanie usunięcia. Zamyka popup.
        /// </summary>
        [RelayCommand]
        protected void CancelDelete()
        {
            IsPopupVisible = false;
        }


        /// <summary>
        /// Komenda do kliknięcia - potwierdzenie usunięcia w okienku popup.
        /// </summary>
        [RelayCommand]
        private async Task ConfirmDeleteAsync()
        {
            await ExecuteConfirmDeleteAsync();
            IsPopupVisible = false;
        }

        /// <summary>
        /// Metoda asynchroniczna robiąca faktyczne usuwanie, do nadpisania w konkretnych ViewModelach.
        /// </summary>
        protected virtual async Task ExecuteConfirmDeleteAsync()
        {
            await Task.CompletedTask; 
        }

        /// <summary>
        /// Pomocnicza metoda do parsowania daty z tekstu na obiekt DateTime w różnych formatach.
        /// </summary>
        /// <param name="dateStr">Tekstowa reprezentacja daty wpisana przez użytkownika.</param>
        /// <param name="date">Wyjściowy obiekt daty.</param>
        /// <returns>Zwraca true, jeśli udało się sparsować datę, w innym wypadku false.</returns>
        protected bool TryParseDate(string dateStr, out DateTime date)
        {
            if (string.IsNullOrWhiteSpace(dateStr))
            {
                date = default;
                return false;
            }
            return DateTime.TryParseExact(dateStr.Trim(), 
                new[] { "dd.MM.yyyy", "yyyy-MM-dd", "d.M.yyyy", "dd/MM/yyyy", "d/M/yyyy" }, 
                System.Globalization.CultureInfo.InvariantCulture, 
                System.Globalization.DateTimeStyles.None, 
                out date) 
                || DateTime.TryParse(dateStr.Trim(), out date);
        }
    }
}


