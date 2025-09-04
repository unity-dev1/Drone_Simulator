using UnityEngine;
using UnityEngine.SceneManagement;

public class HamburgerMenu : MonoBehaviour
{
    public GameObject menuPanel; // Assign this in the Inspector

    private bool isMenuOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        menuPanel.SetActive(isMenuOpen);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    public void InGameHome()
    {
        SceneManager.LoadScene(0); // Loads scene with build index 0
    }
}
