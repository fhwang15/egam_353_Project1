using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 专门为旧Input System设计的Restart按钮脚本
/// 解决UI按钮在旧Input System下不响应的问题
/// </summary>
public class OldInputRestartButton : MonoBehaviour
{
    [Header("Button设置")]
    public Button restartButton;
    
    [Header("键盘快捷键")]
    public KeyCode restartKey = KeyCode.R;
    public bool enableKeyboardRestart = true;
    
    [Header("重置设置")]
    public bool resetTimeScale = true;
    
    void Start()
    {
        // 确保按钮连接
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
            Debug.Log("Restart button connected successfully");
        }
        else
        {
            Debug.LogError("Restart button is not assigned!");
        }
    }
    
    void Update()
    {
        // 键盘快捷键重启
        if (enableKeyboardRestart && Input.GetKeyDown(restartKey))
        {
            Debug.Log($"Restart key ({restartKey}) pressed");
            RestartGame();
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        
        // 重置时间缩放
        if (resetTimeScale)
        {
            Time.timeScale = 1f;
            Debug.Log("Time scale reset to 1");
        }
        
        // 获取当前场景索引
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        Debug.Log($"Current scene index: {currentSceneIndex}");
        
        // 检查场景是否在Build Settings中
        if (currentSceneIndex < 0)
        {
            Debug.LogError("Current scene is not in Build Settings! Please add it to File > Build Settings > Scenes In Build");
            return;
        }
        
        try
        {
            // 重新加载场景
            SceneManager.LoadScene(currentSceneIndex);
            Debug.Log("Scene reloaded successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to reload scene: {e.Message}");
        }
    }
    
    void OnDestroy()
    {
        // 清理事件监听器
        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartGame);
        }
    }
    
    // 公共方法：从其他脚本调用
    public void ForceRestart()
    {
        RestartGame();
    }
}
