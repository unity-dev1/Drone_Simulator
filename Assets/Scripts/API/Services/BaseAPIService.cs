using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using DroneSimulator.API.Interfaces;
using DroneSimulator.API.Config;

namespace DroneSimulator.API.Services
{
    public abstract class BaseAPIService : IAPIService
    {
        protected readonly APIConfig config;
        protected readonly string baseUrl;
        protected string authToken;

        protected BaseAPIService()
        {
            config = APIConfig.Instance;
            baseUrl = config.BaseUrl;
        }

        public virtual async Task<T> GetAsync<T>(string endpoint)
        {
            return await SendRequestAsync<T>(endpoint, UnityWebRequest.kHttpVerbGET);
        }

        public virtual async Task<T> PostAsync<T>(string endpoint, object data)
        {
            return await SendRequestAsync<T>(endpoint, UnityWebRequest.kHttpVerbPOST, data);
        }

        public virtual async Task<T> PutAsync<T>(string endpoint, object data)
        {
            return await SendRequestAsync<T>(endpoint, UnityWebRequest.kHttpVerbPUT, data);
        }

        public virtual async Task<T> DeleteAsync<T>(string endpoint)
        {
            return await SendRequestAsync<T>(endpoint, UnityWebRequest.kHttpVerbDELETE);
        }

        protected void SetAuthToken(string token)
        {
            authToken = token;
        }

        protected virtual async Task<T> SendRequestAsync<T>(string endpoint, string method, object data = null)
        {
            string url = $"{baseUrl}/{endpoint}";
            using (UnityWebRequest request = new UnityWebRequest(url, method))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.timeout = (int)config.Timeout;

                if (data != null)
                {
                    string jsonData = JsonUtility.ToJson(data);
                    request.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(jsonData));
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                if (!string.IsNullOrEmpty(authToken))
                {
                    string authHeader = $"Token {authToken}";
                    request.SetRequestHeader("Authorization", authHeader);
                    Debug.Log($"[API Request] {method} {url} - Using auth header: Authorization: {authHeader}");
                }
                else if (!string.IsNullOrEmpty(config.ApiKey))
                {
                    string authHeader = $"Bearer {config.ApiKey}";
                    request.SetRequestHeader("Authorization", authHeader);
                    Debug.Log($"[API Request] {method} {url} - Using API key header: Authorization: {authHeader}");
                }
                else
                {
                    Debug.LogWarning($"[API Request] {method} {url} - No authorization token or API key available!");
                }

                try
                {
                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string jsonResponse = request.downloadHandler.text;
                        Debug.Log($"API Response: {jsonResponse}");
                        return JsonUtility.FromJson<T>(jsonResponse);
                    }
                    else
                    {
                        Debug.LogError($"API Error: {request.error}");
                        Debug.LogError($"Response Code: {request.responseCode}");
                        Debug.LogError($"Response Text: {request.downloadHandler?.text}");
                        throw new Exception($"API request failed: {request.error}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"API Exception: {ex.Message}");
                    throw;
                }
            }
        }
    }

    public class BypassCertificate : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true; 
        }
    }

}