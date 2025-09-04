using System;
using UnityEngine;
using DroneSimulator.API.Interfaces;
using DroneSimulator.API.Models;
using DroneSimulator.API.Core;

namespace DroneSimulator.API.Services
{
    public class AuthStateService : IAuthService
    {
        private const string AUTH_TOKEN_KEY = "auth_token";
        private const string USER_DATA_KEY = "user_data";

        private UserData currentUser;
        private string authToken;

        public bool IsAuthenticated => !string.IsNullOrEmpty(authToken);
        public string AuthToken => authToken;
        public UserData CurrentUser => currentUser;

        public AuthStateService()
        {
            LoadSavedAuth();
            // Register this service
            APIServiceLocator.Instance.RegisterService<IAuthService>(this);
        }

        public void SetAuthData(string token, UserData user)
        {
            authToken = token;
            currentUser = user;

            PlayerPrefs.SetString(AUTH_TOKEN_KEY, token);
            PlayerPrefs.SetString(USER_DATA_KEY, JsonUtility.ToJson(user));
            PlayerPrefs.Save();
        }

        public void Logout()
        {
            authToken = null;
            currentUser = null;
            PlayerPrefs.DeleteKey(AUTH_TOKEN_KEY);
            PlayerPrefs.DeleteKey(USER_DATA_KEY);
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

        // These methods are required by IAuthService but not used in this implementation
        public System.Threading.Tasks.Task<LoginResponse> LoginAsync(string email, string password)
        {
            throw new NotImplementedException("Use LoginService for API calls");
        }

        public System.Threading.Tasks.Task<SocialLoginResponse> SocialLoginAsync(string email, string username)
        {
            throw new NotImplementedException("Use LoginService for API calls");
        }

        public System.Threading.Tasks.Task<LogoutResponse> LogoutAsync()
        {
            throw new NotImplementedException("Use LoginService for API calls");
        }
    }
} 