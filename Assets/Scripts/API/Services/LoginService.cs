using System;
using System.Threading.Tasks;
using UnityEngine;
using DroneSimulator.API.Interfaces;
using DroneSimulator.API.Models;
using DroneSimulator.API.Core;

namespace DroneSimulator.API.Services
{
    public class LoginService : BaseAPIService, ILoginService
    {
        private const string LOGIN_ENDPOINT = "api/login/";
        private const string SOCIAL_LOGIN_ENDPOINT = "api/social-login/";
        private const string LOGOUT_ENDPOINT = "api/logout/";

        public LoginService()
        {
            // Register this service
            APIServiceLocator.Instance.RegisterService<ILoginService>(this);
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var loginRequest = new LoginRequest
            {
                email = email,
                password = password
            };

            try
            {
                var response = await PostAsync<LoginResponse>(LOGIN_ENDPOINT, loginRequest);
                
                if (response != null && response.data != null && response.data.user != null)
                {
                    // Notify other services about successful login
                    NotifyLoginSuccess(response.data.token, response.data.user);
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Login failed: {ex.Message}");
                throw;
            }
        }

        public async Task<SocialLoginResponse> SocialLoginAsync(string email, string username)
        {
            var request = new SocialLoginRequest
            {
                email = email,
                username = username
            };

            try
            {
                var response = await PostAsync<SocialLoginResponse>(SOCIAL_LOGIN_ENDPOINT, request);

                if (response != null && response.data != null && response.data.user != null)
                {
                    // Notify other services about successful login
                    NotifyLoginSuccess(response.data.token, response.data.user);
                }

                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Social login failed: {ex.Message}");
                throw;
            }
        }

        public async Task<LogoutResponse> LogoutAsync()
        {
            try
            {
                string token = null;
                
                if (APIServiceLocator.Instance.HasService<IAuthService>())
                {
                    var authService = APIServiceLocator.Instance.GetService<IAuthService>();
                    token = authService.AuthToken;
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        SetAuthToken(token);
                        Debug.Log($"Using auth token from AuthService for logout: {token}");
                    }
                    else
                    {
                        Debug.LogWarning("No auth token available from AuthService for logout");
                    }
                }
                else
                {
                    Debug.LogError("AuthService not found in service locator");
                }
                
                // Fallback: try to get token directly from PlayerPrefs if service didn't have it
                if (string.IsNullOrEmpty(token))
                {
                    token = PlayerPrefs.GetString("auth_token", "");
                    if (string.IsNullOrEmpty(token))
                    {
                        token = PlayerPrefs.GetString("AuthToken", "");
                    }
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        SetAuthToken(token);
                        Debug.Log($"Using auth token from PlayerPrefs for logout: {token}");
                    }
                    else
                    {
                        Debug.LogWarning("No auth token found in PlayerPrefs either");
                    }
                }
                
                var response = await PostAsync<LogoutResponse>(LOGOUT_ENDPOINT, null);
                
                // Notify other services about logout
                NotifyLogout();
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Logout failed: {ex.Message}");
                NotifyLogout(); // Still notify logout even if API fails
                throw;
            }
        }

        private void NotifyLoginSuccess(string token, UserData user)
        {
            // Notify auth service
            if (APIServiceLocator.Instance.HasService<IAuthService>())
            {
                var authService = APIServiceLocator.Instance.GetService<IAuthService>();
                if (authService is AuthStateService authStateService)
                {
                    authStateService.SetAuthData(token, user);
                }
            }

            // You can add more notifications here for other services
        }

        private void NotifyLogout()
        {
            // Notify auth service
            if (APIServiceLocator.Instance.HasService<IAuthService>())
            {
                var authService = APIServiceLocator.Instance.GetService<IAuthService>();
                authService.Logout();
            }

            // You can add more notifications here for other services
        }
    }
} 