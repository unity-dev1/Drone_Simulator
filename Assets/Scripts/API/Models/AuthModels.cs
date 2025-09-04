using System;
using UnityEngine;

namespace DroneSimulator.API.Models
{
    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class UserData
    {
        public int id;
        public string username;
        public string email;
        public string full_name;
        public string phone_number;
        public string city;
        public string state_province;
        public string country;
        public string purpose_of_use;
        public bool is_active;
        public string plan;
        public string plan_expiry_date;
        public string last_login_date;
        public bool is_social_login;
        public bool is_paid_user;
        public string created_at;
        public string updated_at;
    }

    [Serializable]
    public class LoginResponseData
    {
        public UserData user;
        public string token;
    }

    [Serializable]
    public class LoginResponse
    {
        public string message;
        public LoginResponseData data;
    }

    // Social login models
    [Serializable]
    public class SocialLoginRequest
    {
        public string email;
        public string username;
    }

    [Serializable]
    public class SocialLoginResponseData
    {
        public UserData user;
        public string token;
    }

    [Serializable]
    public class SocialLoginResponse
    {
        public string status;
        public string message;
        public SocialLoginResponseData data;
    }

    // Logout models
    [Serializable]
    public class LogoutResponse
    {
        public string message;
    }
} 