using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Esseti.Services;
using Models.Other;

namespace Esseti.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly EssetiDbContext _context;
        private readonly ICacheService _cacheService;

        public TripRepository(EssetiDbContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        public async Task<List<Trip>> GetTripsAsync()
        {
            return await _cacheService.GetOrLoadAsync("trips_all", () => _context.Trips.ToListAsync());
        }

        public async Task AddTripAsync(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
            _cacheService.Invalidate("trips_all");
        }

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


