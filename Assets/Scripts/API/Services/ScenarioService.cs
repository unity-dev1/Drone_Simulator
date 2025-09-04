using System;
using System.Threading.Tasks;
using UnityEngine;
using DroneSimulator.API.Interfaces;
using DroneSimulator.API.Models;
using DroneSimulator.API.Core;

namespace DroneSimulator.API.Services
{
    public class ScenarioService : BaseAPIService, IScenarioService
    {
        private const string START_SCENARIO_ENDPOINT = "api/users/start_scenario/";
        private const string END_SCENARIO_ENDPOINT = "api/users/end_scenario/";

        public ScenarioService()
        {
            APIServiceLocator.Instance.RegisterService<IScenarioService>(this);
        }

        public async Task<StartScenarioResponse> StartScenarioAsync(string locationName, string scenarioName, string droneName)
        {
            var request = new StartScenarioRequest
            {
                location_name = locationName,
                scenario_name = scenarioName,
                drone_name = droneName
            };

            try
            {
                // Get auth token from auth service (same as LoginService)
                if (APIServiceLocator.Instance.HasService<IAuthService>())
                {
                    var authService = APIServiceLocator.Instance.GetService<IAuthService>();
                    string token = authService.AuthToken;
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        SetAuthToken(token);
                        Debug.Log($"Using auth token for start scenario: {token}");
                    }
                    else
                    {
                        Debug.LogWarning("No auth token available for start scenario");
                    }
                }
                else
                {
                    Debug.LogError("AuthService not found in service locator");
                }

                var response = await PostAsync<StartScenarioResponse>(START_SCENARIO_ENDPOINT, request);
                
                if (response != null && response.data != null)
                {
                    Debug.Log($"Scenario started successfully. Scenario ID: {response.data.id}");
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Start scenario failed: {ex.Message}");
                throw;
            }
        }

        public async Task<EndScenarioResponse> EndScenarioAsync(string locationName, string scenarioName, string droneName)
        {
            var request = new EndScenarioRequest
            {
                location_name = locationName,
                scenario_name = scenarioName,
                drone_name = droneName
            };

            try
            {
                // Get auth token from auth service (same as LoginService)
                if (APIServiceLocator.Instance.HasService<IAuthService>())
                {
                    var authService = APIServiceLocator.Instance.GetService<IAuthService>();
                    string token = authService.AuthToken;
                    
                    if (!string.IsNullOrEmpty(token))
                    {
                        SetAuthToken(token);
                        Debug.Log($"Using auth token for end scenario: {token}");
                    }
                    else
                    {
                        Debug.LogWarning("No auth token available for end scenario");
                    }
                }
                else
                {
                    Debug.LogError("AuthService not found in service locator");
                }

                var response = await PostAsync<EndScenarioResponse>(END_SCENARIO_ENDPOINT, request);
                
                if (response != null && response.data != null)
                {
                    Debug.Log($"Scenario ended successfully. Scenario ID: {response.data.id}");
                }
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.LogError($"End scenario failed: {ex.Message}");
                throw;
            }
        }
    }
} 