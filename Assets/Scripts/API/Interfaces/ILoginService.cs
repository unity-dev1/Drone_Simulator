using System.Threading.Tasks;
using DroneSimulator.API.Models;

namespace DroneSimulator.API.Interfaces
{
    public interface ILoginService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
        Task<SocialLoginResponse> SocialLoginAsync(string email, string username);
        Task<LogoutResponse> LogoutAsync();
    }
} 