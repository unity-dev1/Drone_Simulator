using Assets.SimpleSignIn.Google.Scripts;
using UnityEngine;

namespace User 
{
    public interface IUserDataProvider 
    {
        UserProfile GetUserProfile();
    }

    public class UserProfile
    {
        public string userName;
        public string email;
        public string subscription;
    }
}

