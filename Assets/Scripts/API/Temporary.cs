using UnityEngine;
using UnityEngine.UI;

public class Temporary : MonoBehaviour
{
    [SerializeField] private Button forgotPassword;
    [SerializeField] private Button register;

    void Start()
    {
        forgotPassword.onClick.AddListener(() =>
        {
            OpenURL("https://www.dronesimulator.pro/auth/forgot-password");
        });

        register.onClick.AddListener(() =>
        {
            OpenURL("https://www.dronesimulator.pro/auth/register");
        });
    }

    void OpenURL(string url) 
    {
        Application.OpenURL(url);
    }
  
}
