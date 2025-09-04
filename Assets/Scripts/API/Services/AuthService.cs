using System;
using System.Threading.Tasks;
using UnityEngine;
using DroneSimulator.API.Interfaces;
using DroneSimulator.API.Models;

namespace DroneSimulator.API.Services
{
    public class AuthService : BaseAPIService, IAuthService
    {
        private const string LOGIN_ENDPOINT = "api/login/";
        private const string SOCIAL_LOGIN_ENDPOINT = "api/social-login/";
        private const string LOGOUT_ENDPOINT = "api/logout/";
        private const string AUTH_TOKEN_KEY = "auth_token";
        private const string USER_DATA_KEY = "user_data";

        private UserData currentUser;
        private string authToken;

        public bool IsAuthenticated => !string.IsNullOrEmpty(authToken);
        public string AuthToken => authToken;
        public UserData CurrentUser => currentUser;

        public AuthService()
        {
            LoadSavedAuth();
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
                
                if (response.data != null && response.data.user != null && response.data.user.username != null) 
                {
                    SelectionManager.Instance.apiCalling.OnAPICall(true, response.data.user.username);
                }
                

                if (response != null && response.data != null && response.data.user != null)
                {
                    SaveAuthData(response.data.token, response.data.user);
                }
                else
                {
                    SelectionManager.Instance.apiCalling.OnAPICall(false, response?.data?.user?.username ?? "unknown");
                    Debug.LogError("Login response is missing required data");
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
                Debug.Log($"Social Login Response: {JsonUtility.ToJson(response)}");

                if (response != null && response.data != null && response.data.user != null)
                {
                    SaveAuthData(response.data.token, response.data.user);
                }
                else
                {
                    Debug.LogError("Social login response is missing required data");
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
                // Set the auth token for this request
                SetAuthToken(authToken);
                
                var response = await PostAsync<LogoutResponse>(LOGOUT_ENDPOINT, null);
                Debug.Log($"Logout Response: {JsonUtility.ToJson(response)}");
                
                // Clear local auth data regardless of API response
                Logout();
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Logout failed: {ex.Message}");
                // Still clear local auth data even if API call fails
                Logout();
                throw;
            }
        }

        public void Logout()
        {
            authToken = null;
            currentUser = null;
            PlayerPrefs.DeleteKey(AUTH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(USER_DATA_KEY);
            PlayerPrefs.Save();
        }

        private void SaveAuthData(string token, UserData user)
        {
            authToken = token;
            currentUser = user;

            PlayerPrefs.SetString(AUTH_TOKEN_KEY, token);
            PlayerPrefs.SetString(USER_DATA_KEY, JsonUtility.ToJson(user));
            PlayerPrefs.Save();
        }

        private void LoadSavedAuth()
        {
            if (PlayerPrefs.HasKey(AUTH_TOKEN_KEY))
            {
                authToken = PlayerPrefs.GetString(AUTH_TOKEN_KEY);
                string userJson = PlayerPrefs.GetString(USER_DATA_KEY);
                currentUser = JsonUtility.FromJson<UserData>(userJson);
            }
        }
    }
} 