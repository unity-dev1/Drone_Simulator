using UnityEngine;
using DroneSimulator.API.Services;
using DroneSimulator.API.Models;
using User;
using System;
using Newtonsoft.Json;
using TMPro;
using DroneSimulator.API.Interfaces;
using DroneSimulator.API.Core;

namespace DroneSimulator.API
{
    public class SocialLoginManager : MonoBehaviour, IUserDataProvider, IAPIModel
    {
        public TextMeshProUGUI feedbackText;

        private AuthService authService;

        private string authToken;

        public event Action<bool, string> OnLoginComplete;

        public event Action<UserData> OnUserDataReceived;
        private UserProfile userProfile;

        private void Awake()
        {
            authService = new AuthService();
        }

        public void LoginWithSocial(string email, string name) 
        {
            OnSocialLoginButtonClicked(email, name);
        }

        private async void OnSocialLoginButtonClicked(string _email, string _name)
        {
            string email = _email;
            string username = _name;
            if (feedbackText != null)
                feedbackText.text = "Logging in with social account...";

            try
            {
                var response = await authService.SocialLoginAsync(email, username);
                if (response != null && response.data != null && response.data.user != null)
                {
                    string userCurrentPlan = response.data.user.plan;
                    userProfile = new UserProfile
                    {
                        userName = response.data.user.username,
                        email = response.data.user.email,
                        subscription = char.ToUpper(userCurrentPlan[0]) + userCurrentPlan.Substring(1),
                    };

                    authToken = response.data.token;
                    OnUserDataReceived?.Invoke(response.data.user);

                    OnLoginComplete?.Invoke(true, "Login successful");

                    PlayerPrefs.SetString("auth_token", authToken);
                    PlayerPrefs.SetString("user_data", JsonUtility.ToJson(response.data.user));
                    
                    PlayerPrefs.SetString("AuthToken", authToken);
                    string userData = JsonConvert.SerializeObject(response.data.user);
                    PlayerPrefs.SetString("UserData", userData);
                    PlayerPrefs.Save();
                    
                    if (APIServiceLocator.Instance.HasService<IAuthService>())
                    {
                        var authStateService = APIServiceLocator.Instance.GetService<IAuthService>();
                        if (authStateService is AuthStateService authState)
                        {
                            authState.SetAuthData(authToken, response.data.user);
                            Debug.Log("AuthStateService updated with login data");
                        }
                    }
                    
                    string userPlan = response.data.user.plan?.ToLower().Trim();
                    SelectionManager.Instance.userPlan = char.ToUpper(userPlan[0]) + userPlan.Substring(1).ToLower();
                    SelectionManager.Instance.isPro = userPlan == "trial" || userPlan == "premium";

                    if (feedbackText != null)
                        feedbackText.text = $"Welcome, {response.data.user.full_name}!";
                }
                else
                {
                    OnLoginComplete?.Invoke(false, response?.message);
                    if (feedbackText != null)
                        feedbackText.text = $"Social login failed: {response?.message ?? "Unknown error"}";
                }
            }
            catch (System.Exception ex)
            {
                OnLoginComplete?.Invoke(false, ex.Message);

                if (feedbackText != null)
                    feedbackText.text = $"Error: {ex.Message}";
            }
        }

        public UserProfile GetUserProfile()
        {
            return userProfile;
        }

        public string GetAuthToken()
        {
            if (string.IsNullOrEmpty(authToken))
            {
                authToken = PlayerPrefs.GetString("auth_token", "");
                if (string.IsNullOrEmpty(authToken))
                {
                    authToken = PlayerPrefs.GetString("AuthToken", "");
                }
            }
            return authToken;
        }

        public void Logout()
        {
            authToken = null;
            PlayerPrefs.DeleteKey("auth_token");
            PlayerPrefs.DeleteKey("user_data");
            PlayerPrefs.DeleteKey("AuthToken");
            PlayerPrefs.DeleteKey("UserData");
            PlayerPrefs.Save();
        }

        public void OnReceivedUserData(UserProfile userProfile)
        {
           this.userProfile = userProfile;
        }
    }
}