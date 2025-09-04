using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;

public class NavBarController : MonoBehaviour
{
    [System.Serializable]
    public class NavButton
    {
        public GameObject buttonGroup;        // e.g., HomeButtonGrp
        public Image iconImage;              // Icon
        public TextMeshProUGUI text;         // Text
        public GameObject targetPage;        // The main page to show when clicked
        public Sprite activeSprite;
        public Sprite inactiveSprite;
    }

    public RectTransform highlighter;        // The movable highlight image
    public List<GameObject> allPages;        // Every possible page, including unrelated ones
    public Color activeTextColor = Color.black;
    public Color inactiveTextColor = Color.white;

    public NavButton[] navButtons;

    public GameObject scenarioButton;

    public Button profileButton;
    public Button settingButton;
    public MenuTabs menuTab;
    private IUserProfile showProfile;

    [SerializeField] private TextMeshProUGUI subscriptionText;

    private void Start()
    {
        showProfile = menuTab as IUserProfile;

        subscriptionText.text = SelectionManager.Instance.userPlan;

        profileButton.onClick.AddListener(() =>
        {
            showProfile.OpenProfile();
            settingButton.onClick.Invoke();
        });
    }

    private void OnEnable()
    {
        SelectionManager.Instance.onScreenIndexChange += OnScreenIndexChange;
    }

    private void OnDisable()
    {
        SelectionManager.Instance.onScreenIndexChange -= OnScreenIndexChange;
    }

    public void OnNavButtonClicked(int index)
    {
        // 1. Deactivate all pages
        foreach (GameObject page in allPages)
        {
            if (page != null)
                page.SetActive(false);
        }

        for (int i = 0; i < navButtons.Length; i++)
        {
            bool isActive = i == index;

            // 2. Activate only selected page
            if (isActive && navButtons[i].targetPage != null)
                navButtons[i].targetPage.SetActive(true);

            // 3. Swap icon and text color
            navButtons[i].iconImage.sprite = isActive ? navButtons[i].activeSprite : navButtons[i].inactiveSprite;
            navButtons[i].text.color = isActive ? activeTextColor : inactiveTextColor;

            // 4. Move highlighter
            if (isActive && highlighter != null)
            {
                highlighter.position = navButtons[i].buttonGroup.transform.position;
            }
        }
    }

    public void EnableScenario(int index)
    {
        // 1. Deactivate all pages
        foreach (GameObject page in allPages)
        {
            if (page != null)
                page.SetActive(false);
        }

        

        for (int i = 0; i < navButtons.Length; i++)
        {
            bool isActive = i == index;

            // 2. Activate only selected page
            if (isActive && navButtons[i].targetPage != null)
                navButtons[i].targetPage.SetActive(true);

            // 3. Swap icon and text color
            navButtons[i].iconImage.sprite = isActive ? navButtons[i].activeSprite : navButtons[i].inactiveSprite;
            navButtons[i].text.color = isActive ? activeTextColor : inactiveTextColor;

            // 4. Move highlighter
            if (isActive && highlighter != null)
            {
                highlighter.position = navButtons[i].buttonGroup.transform.position;
            }
        }
    }

    private void OnScreenIndexChange(int screenIndex)
    {
        scenarioButton.SetActive(screenIndex >= 3);
    }

    private void NavigateSreen(int screenIndex) 
    {
        if (screenIndex == 0) return;
        if (screenIndex == 1)
        {
            SelectionManager.Instance.UpdateActiveSceneIndex(1);
        }
        else 
        {
            SelectionManager.Instance.UpdateActiveSceneIndex(screenIndex - 1);
        }


        allPages
            .Select((page, index) => new {page, index })
            .ToList().ForEach(x => x.page.SetActive(x.index == screenIndex));
    }

    public void NavigateBackScreen() 
    {
        switch (SelectionManager.Instance.activeScreenIndex)
        {
            case 1:
                NavigateSreen(1);
                break;
            case 2:
                NavigateSreen(1);
                break;
            case 3:
                NavigateSreen(2);
                break;
            case 4:
                NavigateSreen(3);
                break;
        }
    }
}
