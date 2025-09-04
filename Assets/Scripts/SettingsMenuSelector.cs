using UnityEngine;
using UnityEngine.UI;

public class MenuTabs : MonoBehaviour, IUserProfile
{
    [Header("Panels")]
    public GameObject audioPanel;
    public GameObject displayPanel;
    public GameObject mappingPanel;
    public GameObject profilePanel;

    [Header("Buttons")]
    public Button audioButton;
    public Button displayButton;
    public Button mappingButton;
    public Button profileButton;

    [Header("Colors")]
    public Color selectedColor = new Color(1f, 0.349f, 0f); // #FF5900
    public Color defaultColor = Color.white;

    bool isProfile;

    void Start()
    {
        // Assign button callbacks
        audioButton.onClick.AddListener(() => ShowPanel("Audio"));
        displayButton.onClick.AddListener(() => ShowPanel("Display"));
        mappingButton.onClick.AddListener(() => ShowPanel("Mapping"));
        profileButton.onClick.AddListener(() => ShowPanel("Profile"));

        if (!isProfile) 
        {
            ShowPanel("Display");
            if (isProfile) isProfile = false;
        }
    }

    void ShowPanel(string panelName)
    {
        // Toggle panels
        audioPanel.SetActive(panelName == "Audio");
        displayPanel.SetActive(panelName == "Display");
        mappingPanel.SetActive(panelName == "Mapping");
        profilePanel.SetActive(panelName == "Profile");

        // Change button colors
        SetButtonColor(audioButton, panelName == "Audio");
        SetButtonColor(displayButton, panelName == "Display");
        SetButtonColor(mappingButton, panelName == "Mapping");
        SetButtonColor(profileButton, panelName == "Profile");
    }

    void SetButtonColor(Button button, bool isSelected)
    {
        var colors = button.colors;
        colors.normalColor = isSelected ? selectedColor : defaultColor;
        colors.selectedColor = colors.normalColor;
        button.colors = colors;
    }

    public void OpenProfile()
    {
        isProfile = true;
        ShowPanel("Profile");
    }
}
