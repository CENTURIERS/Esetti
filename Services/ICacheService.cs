using System;
using System.Threading.Tasks;

namespace Esseti.Services
{
    /// <summary>
    /// Interfejs do buforowania w pamięci (cache'owania).
    /// Pomaga oszczędzać zasoby i nie odpytywać bazy danych za każdym razem o to samo.
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Pobiera dane z cache po określonym kluczu. 
        /// Jeśli danych nie ma lub wygasły, odpala funkcję ładującą (loader), zapisuje wynik w cache i go zwraca.
        /// </summary>
        /// <typeparam name="T">Typ zwracanych danych.</typeparam>
        /// <param name="key">Unikalny klucz do identyfikacji wpisu w cache.</param>
        /// <param name="loader">Delegat (metoda) pobierająca dane, jeśli w cache jest pusto.</param>
        /// <param name="ttl">Czas życia wpisu w cache (opcjonalny, jak nie podasz, to idzie domyślny).</param>
        /// <returns>Zwraca zapamiętane albo świeżo załadowane dane.</returns>
        Task<T> GetOrLoadAsync<T>(string key, Func<Task<T>> loader, TimeSpan? ttl = null);

        /// <summary>
        /// Usuwa konkretny wpis z cache po kluczu (inwalidacja wpisu).
        /// </summary>
        /// <param name="key">Klucz wpisu, który chcemy wywalić.</param>
        void Invalidate(string key);

        /// <summary>
        /// Czyści cały cache do zera, usuwając wszystkie zapamiętane wpisy.
        /// </summary>
        void InvalidateAll();
    }
}


