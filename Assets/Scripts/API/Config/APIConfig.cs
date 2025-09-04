using UnityEngine;

namespace DroneSimulator.API.Config
{
    [CreateAssetMenu(fileName = "APIConfig", menuName = "DroneSimulator/API/Configuration")]
    public class APIConfig : ScriptableObject
    {
        [Header("API Settings")]
        [SerializeField] private string baseUrl = "https://34-47-194-149.nip.io";
      //  [SerializeField] private string baseUrl = "https://api.example.com"; // Production base url
        [SerializeField] private string apiKey = "";
        [SerializeField] private float timeout = 30f;
        [SerializeField] private int maxRetries = 3;

        public string BaseUrl => baseUrl;
        public string ApiKey => apiKey;
        public float Timeout => timeout;
        public int MaxRetries => maxRetries;

        private static APIConfig instance;
        public static APIConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<APIConfig>("APIConfig");
                    if (instance == null)
                    {
                        Debug.LogError("APIConfig not found in Resources folder!");
                    }
                }
                return instance;
            }
        }
    }
} 