using UnityEngine;
using UnityEngine.UI;
using DroneSimulator.API.Services;
using DroneSimulator.API.Interfaces;
using TMPro;
using User;
using System;

namespace DroneSimulator.API
{
    public class LoginManager : MonoBehaviour
    {
        public TMP_InputField emailInput;
        public TMP_InputField passwordInput;
        public Button loginButton;
        public TextMeshProUGUI feedbackText;

        private IAuthService authService;

        private IAPIModel profileModel;
        [SerializeField] private SocialLoginManager socialLoginManager;

        private void Awake()
        {
            authService = new AuthService();
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }

        private async void OnLoginButtonClicked()
        {
            string email = emailInput.text;
            string password = passwordInput.text;
            feedbackText.text = "Logging in...";

            try
            {
                var response = await authService.LoginAsync(email, password);
                if (response != null && response.data != null && response.data.user != null)
                {
                    feedbackText.text = $"Welcome, {response.data.user.full_name}!";

                    string userCurrentPlan = response.data.user.plan;
                    UserProfile userProfile = new UserProfile
                    {
                        userName = response.data.user.username,
                        email = response.data.user.email,
                        subscription = char.ToUpper(userCurrentPlan[0]) + userCurrentPlan.Substring(1),
                    };
                    
                    string userPlan = response.data.user.plan?.ToLower().Trim();
                    SelectionManager.Instance.userPlan = char.ToUpper(userPlan[0]) + userPlan.Substring(1).ToLower();
                    SelectionManager.Instance.isPro = userPlan == "trial" || userPlan == "premium";

                    profileModel = socialLoginManager as IAPIModel;

                    profileModel.OnReceivedUserData(userProfile);
                }
                else
                {
                    feedbackText.text = $"Login failed: {response?.message ?? "Unknown error"}";
                }
            }
            catch (System.Exception ex)
            {
                feedbackText.text = $"Error: {ex.Message}";
            }
        }
    }
}