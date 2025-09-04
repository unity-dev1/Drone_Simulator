using System.Threading.Tasks;
using DroneSimulator.API.Models;

namespace DroneSimulator.API.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
        Task<SocialLoginResponse> SocialLoginAsync(string email, string username);
        Task<LogoutResponse> LogoutAsync();
        void Logout();
        bool IsAuthenticated { get; }
        string AuthToken { get; }
        UserData CurrentUser { get; }
    }
} 