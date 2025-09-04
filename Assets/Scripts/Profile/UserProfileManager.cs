using UnityEngine;
using DroneSimulator.API;

namespace User 
{
    public class UserProfileManager : MonoBehaviour
    {
        [SerializeField] UserProfileUI userProfileUI;
        [SerializeField] SocialLoginManager userData;
        IUserDataProvider userDataProvider;

        private void Awake()
        {
            userProfileUI = GetComponent<UserProfileUI>();
            userDataProvider = userData as IUserDataProvider;
        }

        private void OnEnable()
        {
            userProfileUI.UpdateUI(userDataProvider.GetUserProfile());
        }
    }

}
