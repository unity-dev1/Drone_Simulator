using System;
using UnityEngine;
using UnityEngine.UI;
using Assets.SimpleSignIn.Google.Scripts;
using TMPro;
using DroneSimulator.API;
using DroneSimulator.API.Interfaces;
using DroneSimulator.API.Core;
using User;

public class GoogleSignIn : MonoBehaviour, IAPICalling
{
    [SerializeField] private SocialLoginManager socialLoginManager;
    public GoogleAuth GoogleAuth;
    public Button googleLogin;
    public string userName;
    string logs;
    string output;

    public GameObject signInScreen;
    public GameObject home;
    public GameObject navBar;
    public GameObject location;
    public GameObject setting;
    public GameObject drone;
    public GameObject scenarioScreen;

    public TextMeshProUGUI userNameText;
    public TextMeshProUGUI loginMessage;


    private void OnEnable()
    {
        
        if (SelectionManager.Instance.isSignIN)
        {
           
            signInScreen.SetActive(false);
            home.SetActive(true);
            navBar.SetActive(true);
            userNameText.text = SelectionManager.Instance.currentUserName;
            DeactivateScreen();
            SelectionManager.Instance?.UpdateActiveSceneIndex(1);
        }
        else 
        {
           
            signInScreen.SetActive(true);
            home.SetActive(false);
            navBar.SetActive(false);

            DeactivateScreen();
            SelectionManager.Instance?.UpdateActiveSceneIndex(0);
        }

        loginMessage.text = "";
        socialLoginManager.OnLoginComplete += OnLogin;

    }

    private void OnDisable()
    {
        loginMessage.text = "";
        socialLoginManager.OnLoginComplete -= OnLogin;
    }


    void Start()
    {
        APIManager.Instance.InitializeServices();
        
        SelectionManager.Instance.apiCalling = this as IAPICalling;

        Application.logMessageReceived += (condition, _, _) => logs += condition + '\n';
        GoogleAuth = new GoogleAuth();
        GoogleAuth.TryResume(OnSignIn, OnGetTokenResponse);

        googleLogin.onClick.AddListener(SignIn);
    }

    public void SignIn()
    {
        GoogleAuth.SignIn(OnSignIn, caching: true);
    }

    public void SignOut()
    {
        try
        {
            GetComponent<LogoutManager>().OnLogoutButtonClicked();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"API logout failed, proceeding with local logout: {ex.Message}");
        }
        
        SelectionManager.Instance.UpdateActiveSceneIndex(0);
        loginMessage.text = "";
        GoogleAuth.SignOut(revokeAccessToken: true);
        SelectionManager.Instance.isSignIN = false;
        signInScreen.SetActive(true);
        home.SetActive(false);
        navBar.SetActive(false);
        DeactivateScreen();
        socialLoginManager.Logout();
        
        Debug.Log("Local logout completed");
    }

    public void GetAccessToken()
    {
        GoogleAuth.GetTokenResponse(OnGetTokenResponse);
    }


    private void OnSignIn(bool success, string error, UserInfo userInfo)
    {

        if (success)
        {
            SelectionManager.Instance.UpdateActiveSceneIndex(1);
            socialLoginManager.LoginWithSocial(userInfo.email, userInfo.name);
            userName = userInfo.name;
        }

        Debug.Log(success ? $"Hello, {userInfo.name}!" : error);
    }

    private void OnLogin(bool success, string message)
    {
        if (success)
        {
            SelectionManager.Instance.isSignIN = true;
            signInScreen.SetActive(false);
            home.SetActive(true);
            navBar.SetActive(true);
            userNameText.text = userName;
            SelectionManager.Instance.currentUserName = userName;

        }
        else 
        {
            SignOut();
        }

        loginMessage.text = message;
    }


    private void OnGetTokenResponse(bool success, string error, TokenResponse tokenResponse)
    {
        Debug.Log(success ? $"Access token: {tokenResponse.AccessToken}" : error);

        if (!success) return;

        var jwt = new JWT(tokenResponse.IdToken);

        Debug.Log($"JSON Web Token (JWT) Payload: {jwt.Payload}");

        jwt.ValidateSignature(GoogleAuth.ClientId, OnValidateSignature);
    }

    private void OnValidateSignature(bool success, string error)
    {
      //  output += Environment.NewLine;
       // output += success ? "JWT signature validated" : error;
    }

    private void DeactivateScreen() 
    {
        location.SetActive(false);
        setting.SetActive(false);
        drone.SetActive(false);
        scenarioScreen.SetActive(false);
    }

    public void OnAPICall(bool success, string userName)
    {
        if (success)
        {
            SelectionManager.Instance.isSignIN = true;
            signInScreen.SetActive(false);
            home.SetActive(true);
            navBar.SetActive(true);
            userNameText.text = userName;
            SelectionManager.Instance.currentUserName = userName;

        }
        else
        {
            SignOut();
        }
    }
}
