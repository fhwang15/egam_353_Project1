using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Reloads the active scene when a UI Button is pressed.
/// Attach to any GameObject. Optionally assign the Button to auto-wire,
/// or call RestartScene() from the Button's OnClick in the Inspector.
/// </summary>
public class RestartOnUIButton : MonoBehaviour
{
    [Header("Optional: auto-wire this Button")]
    [SerializeField] private Button button;

    [Header("Reset timeScale before reload (useful if you paused the game)")]
    [SerializeField] private bool resetTimeScale = true;

    private void Awake()
    {
        if (button != null)
            button.onClick.AddListener(RestartScene);
    }

    public void RestartScene()
    {
        Debug.Log("Restart button clicked!");
        
        if (resetTimeScale) 
        {
            Time.timeScale = 1f;
            Debug.Log("Time scale reset to 1");
        }
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log($"Loading scene with index: {currentSceneIndex}");
        
        try
        {
            SceneManager.LoadScene(currentSceneIndex);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene: {e.Message}");
            Debug.LogError("Make sure the scene is added to Build Settings!");
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(RestartScene);
    }
}
