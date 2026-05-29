using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Esseti.Data;
using Esseti.Repositories.Interfaces;
using Models.Other;

namespace Esseti.Repositories
{
    public class TripRepository : ITripRepository
    {
        private readonly EssetiDbContext _context;

        public TripRepository(EssetiDbContext context)
        {
            _context = context;
        }

        public async Task<List<Trip>> GetTripsAsync()
        {
            return await _context.Trips.ToListAsync();
        }

        public async Task AddTripAsync(Trip trip)
        {
            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();
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
            }
        }

        public async Task DeleteTripAsync(int tripId)
        {
            var trip = await _context.Trips.FindAsync(tripId);
            if (trip != null)
            {
                _context.Trips.Remove(trip);
                await _context.SaveChangesAsync();
            }
        }
    }
}
