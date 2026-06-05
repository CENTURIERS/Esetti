using System.Collections.Generic;
using System.Threading.Tasks;
using Models.Other;

namespace Esseti.Repositories.Interfaces
{
    /// <summary>
    /// Interfejs repozytorium odpowiedzialnego za operacje na wyjazdach/wycieczkach koła.
    /// </summary>
    public interface ITripRepository
    {
        /// <summary>
        /// Pobiera listę wszystkich wyjazdów.
        /// </summary>
        Task<List<Trip>> GetTripsAsync();

        /// <summary>
        /// Dodaje nowy wyjazd do bazy danych.
        /// </summary>
        Task AddTripAsync(Trip trip);

        /// <summary>
        /// Aktualizuje dane istniejącego wyjazdu.
        /// </summary>
        Task UpdateTripAsync(Trip trip);

        /// <summary>
        /// Usuwa wyjazd na podstawie jego identyfikatora.
        /// </summary>
        Task DeleteTripAsync(int tripId);
    }
}


