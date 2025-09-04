using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace User
{
    public class UserProfileUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI emailText;
        [SerializeField] private TextMeshProUGUI emailSubscription;
        [SerializeField] private Button contactButton;

        private void Start()
        {
            contactButton.onClick.AddListener(() =>
            {
                Application.OpenURL("https://www.dronesimulator.pro/contact");
            });
        }

        public void UpdateUI(UserProfile userProfile)
        {
            usernameText.text = userProfile.userName;
            emailText.text = userProfile.email;
            emailSubscription.text = userProfile.subscription;
        }

    }
}

