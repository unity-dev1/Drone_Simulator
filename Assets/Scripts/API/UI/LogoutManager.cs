using DroneSimulator.API.Core;
using System;
using UnityEngine;


namespace DroneSimulator.API
{
    public class LogoutManager : MonoBehaviour
    {
        public async void OnLogoutButtonClicked()
        {
            try
            {
                var loginService = APIManager.Instance.GetLoginService();
                var response = await loginService.LogoutAsync();

                Debug.Log($"Logout successful: {response.message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Logout failed: {ex.Message}");
            }
        }
    }
}