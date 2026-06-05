using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Esseti.Services
{
    /// <summary>
    /// Konkretna implementacja serwisu do cache'owania w pamięci.
    /// Bazuje na <see cref="ConcurrentDictionary{TKey,TValue}"/>, więc jest bezpieczna wątkowo (thread-safe).
    /// </summary>
    public class MemoryCacheService : ICacheService
    {
        /// <summary>
        /// Słownik przechowujący nasze wpisy w cache. Używamy ConcurrentDictionary, żeby nie wywaliło się przy wielu wątkach.
        /// </summary>
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

        /// <summary>
        /// Domyślny czas życia wpisu w cache (ustawiony na sztywno na 5 minut, jak w wymaganiach).
        /// </summary>
        private readonly TimeSpan _defaultTtl = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Pobiera dane z cache po kluczu. Jak ich nie ma lub minął ich czas życia (TTL),
        /// to pobiera je za pomocą funkcji loader, wrzuca do słownika i zwraca.
        /// </summary>
        /// <typeparam name="T">Typ danych, które przechowujemy.</typeparam>
        /// <param name="key">Klucz, pod którym szukamy danych.</param>
        /// <param name="loader">Funkcja ładująca dane z bazy/API, jak nie ma ich w cache.</param>
        /// <param name="ttl">Opcjonalny czas ważności wpisu.</param>
        /// <returns>Zwraca dane z cache lub świeżo załadowane.</returns>
        public async Task<T> GetOrLoadAsync<T>(string key, Func<Task<T>> loader, TimeSpan? ttl = null)
        {
            var now = DateTime.UtcNow;
            if (_cache.TryGetValue(key, out var entry) && entry.Expiry > now)
            {
                if (entry.Value is T typedValue)
                {
                    return typedValue;
                }
            }

            var loadedData = await loader();
            var expiry = now + (ttl ?? _defaultTtl);
            _cache[key] = new CacheEntry(loadedData!, expiry);

            return loadedData;
        }

        /// <summary>
        /// Usuwa wpis o podanym kluczu ze słownika cache.
        /// </summary>
        /// <param name="key">Klucz wpisu, który chcemy usunąć.</param>
        public void Invalidate(string key)
        {
            _cache.TryRemove(key, out _);
        }

        /// <summary>
        /// Czyści cały słownik z danymi. Przydatne przy wylogowaniu albo restarcie danych.
        /// </summary>
        public void InvalidateAll()
        {
            _cache.Clear();
        }

        /// <summary>
        /// Prywatna klasa reprezentująca pojedynczy wpis w cache.
        /// Trzyma sam obiekt oraz informację, do kiedy jest ważny.
        /// </summary>
        private class CacheEntry
        {
            /// <summary>
            /// Wartość wpisu (jako object, bo cache trzyma różne typy).
            /// </summary>
            public object Value { get; }

            /// <summary>
            /// Data i czas (UTC), po którym ten wpis uznajemy za przedawniony (wygasły).
            /// </summary>
            public DateTime Expiry { get; }

            /// <summary>
            /// Tworzy nowy wpis do cache.
            /// </summary>
            /// <param name="value">Wartość do zapamiętania.</param>
            /// <param name="expiry">Data wygaśnięcia wpisu.</param>
            public CacheEntry(object value, DateTime expiry)
            {
                Value = value;
                Expiry = expiry;
            }
        }
    }
}


