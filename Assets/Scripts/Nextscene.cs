using UnityEngine;
using UnityEngine.SceneManagement;

public class Nextscene : MonoBehaviour
{
    // Public function to go to the next scene by index
    public void LoadNextScene()
    {
        // Get current scene index
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;

        // Calculate next scene index
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if next scene exists in build settings
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            // Load the next scene
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // Optional: loop back to first scene if we're at the end
            SceneManager.LoadScene(0);
            Debug.Log("Reached the last scene, looping back to scene 0");
        }
    }
}
