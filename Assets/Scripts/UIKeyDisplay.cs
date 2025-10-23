using UnityEngine;

/// <summary>
/// 按下 '=' 键时显示 targetUI，松开时隐藏。
/// 兼容任意 GameObject（Image、Panel、整个Canvas子树都行）
/// </summary>
public class ShowUIOnEquals : MonoBehaviour
{
    [Header("拖入你要显示/隐藏的 UI 根节点")]
    public GameObject targetUI;

    [Header("启动时是否默认隐藏")]
    public bool hideOnStart = true;

    void Start()
    {
        if (targetUI != null && hideOnStart)
        {
            targetUI.SetActive(false);
        }
    }

    void Update()
    {
        if (targetUI == null) return;

        // 按住 '=' 键就显示，松开就隐藏
        bool isHeld = Input.GetKey(KeyCode.Equals);
        if (targetUI.activeSelf != isHeld)
        {
            targetUI.SetActive(isHeld);
        }
    }
}
