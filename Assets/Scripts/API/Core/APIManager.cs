using UnityEngine;
using DroneSimulator.API.Services;
using DroneSimulator.API.Interfaces;

namespace DroneSimulator.API.Core
{
    public class APIManager : MonoBehaviour
    {
        private static APIManager instance;
        
        [Header("Services")]
        [SerializeField] private bool initializeOnStart = true;

        public static APIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<APIManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("APIManager");
                        instance = go.AddComponent<APIManager>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
                return;
            }

            if (initializeOnStart)
            {
                InitializeServices();
            }
        }

        public void InitializeServices()
        {
            Debug.Log("Initializing API Services...");
            
            var authStateService = new AuthStateService();
            var loginService = new LoginService();
            var scenarioService = new ScenarioService();
            
            Debug.Log("API Services initialized successfully!");
        }

        public T GetService<T>() where T : class
        {
            return APIServiceLocator.Instance.GetService<T>();
        }

        public bool HasService<T>() where T : class
        {
            return APIServiceLocator.Instance.HasService<T>();
        }

        public ILoginService GetLoginService()
        {
            return GetService<ILoginService>();
        }

        public IAuthService GetAuthService()
        {
            return GetService<IAuthService>();
        }

        public IScenarioService GetScenarioService()
        {
            return GetService<IScenarioService>();
        }
    }
} 