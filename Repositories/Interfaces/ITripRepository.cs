using System.Collections.Generic;
using System.Threading.Tasks;
using Models.Other;

namespace Esseti.Repositories.Interfaces
{
    public interface ITripRepository
    {
        Task<List<Trip>> GetTripsAsync();
        Task AddTripAsync(Trip trip);
        Task UpdateTripAsync(Trip trip);
        Task DeleteTripAsync(int tripId);
    }
}


