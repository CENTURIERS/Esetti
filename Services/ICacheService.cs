using System;
using System.Threading.Tasks;

namespace Esseti.Services
{
    public interface ICacheService
    {
        Task<T> GetOrLoadAsync<T>(string key, Func<Task<T>> loader, TimeSpan? ttl = null);
        void Invalidate(string key);
        void InvalidateAll();
    }
}


