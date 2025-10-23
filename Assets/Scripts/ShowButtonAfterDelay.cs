using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Hides a UI Button on start, shows it after a delay (default 120s),
/// and restarts the current scene when the button is clicked.
/// </summary>
public class DelayedRestartButton : MonoBehaviour
{
    [Header("Target UI")]
    [SerializeField] private Button targetButton;

    [Header("Timing (seconds)")]
    [SerializeField] private float delaySeconds = 120f;

    [Header("Behavior")]
    [SerializeField] private bool hideOnStart = true;   // start hidden until delay elapses
    [SerializeField] private bool useRealtime = true;   // unaffected by Time.timeScale
    [SerializeField] private bool resetTimeScale = true; // set timeScale=1 before reload

    private void Awake()
    {
        if (targetButton == null)
        {
            Debug.LogWarning("[DelayedRestartButton] No Button assigned.");
            return;
        }
        // Wire the click handler (works even if the button GameObject is inactive)
        targetButton.onClick.AddListener(RestartScene);
    }

    private void Start()
    {
        if (targetButton == null) return;

        if (hideOnStart) targetButton.gameObject.SetActive(false);

        if (useRealtime)
            StartCoroutine(RevealAfterDelayRealtime());
        else
            StartCoroutine(RevealAfterDelayScaled());
    }

    private IEnumerator RevealAfterDelayRealtime()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delaySeconds));
        if (targetButton != null) targetButton.gameObject.SetActive(true);
    }

    private IEnumerator RevealAfterDelayScaled()
    {
        float t = 0f;
        float d = Mathf.Max(0f, delaySeconds);
        while (t < d)
        {
            t += Time.deltaTime;
            yield return null;
        }
        if (targetButton != null) targetButton.gameObject.SetActive(true);
    }

    public void RestartScene()
    {
        Debug.Log("1");
        if (resetTimeScale) Time.timeScale = 1f;
        Debug.Log("2");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //private void OnDestroy()
    //{
    //    if (targetButton != null)
    //        targetButton.onClick.RemoveListener(RestartScene);
    //}
}
