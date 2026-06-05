using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Models.Other;

namespace Esseti.Repositories
{
    /// <summary>
    /// Klasa implementująca repozytorium do obsługi wyjazdów w bazie danych SQLite.
    /// Wykorzystuje serwis pamięci podręcznej (Cache) w celu optymalizacji pobierania danych.
    /// </summary>
    public class TripRepository : ITripRepository
    {
        private readonly EssetiDbContext _context;
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Inicjalizuje nową instancję repozytorium wyjazdów z podanym kontekstem bazy danych i serwisem cache.
        /// </summary>
        /// <param name="context">Kontekst bazy danych EF Core.</param>
        /// <param name="cacheService">Serwis pamięci podręcznej do cachowania wyników zapytań.</param>
        public TripRepository(EssetiDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        /// <inheritdoc />
        public async Task<List<Trip>> GetTripsAsync()
        {
            return await _cacheService.GetOrLoadAsync("trips_all", () => _context.Trips.ToListAsync());
        }

        /// <inheritdoc />
        public async Task AddTripAsync(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            _cacheService.Invalidate("trips_all");
        }

        /// <inheritdoc />
        public async Task UpdateTripAsync(Trip trip)
        {
            var existing = await _context.Trips.FindAsync(trip.TripId);
            if (existing != null)
            {
                existing.Name = trip.Name;
                existing.Description = trip.Description;
                existing.Date = trip.Date;
                await _context.SaveChangesAsync();
                _cacheService.Invalidate("trips_all");
            }
        }

        /// <inheritdoc />
        public async Task DeleteTripAsync(int tripId)
        {
            var trip = await _context.Trips.FindAsync(tripId);
            if (trip != null)
            {
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
                _cacheService.Invalidate("trips_all");
            }
        }
    }
}


