using System.Threading.Tasks;

namespace DroneSimulator.API.Interfaces
{
    public interface IAPIService
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task<T> DeleteAsync<T>(string endpoint);
    }
} 