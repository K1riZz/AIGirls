using UnityEngine;

/// <summary>
/// 这个脚本专门用于处理宠物预制体内部UI按钮的点击事件，
/// 它会调用场景中GameManager的单例。
/// </summary>
public class StoryModeButtonHandler : MonoBehaviour
{
    /// <summary>
    /// 这个方法将被UI按钮的OnClick事件调用。
    /// </summary>
    public void TriggerEnterStoryMode()
    {
        // 检查GameManager实例是否存在
        if (GameManager.Instance != null)
        {
            // 调用GameManager的单例方法
            GameManager.Instance.EnterStoryMode();
        }
        else
        {
            Debug.LogError("GameManager.Instance not found! Cannot enter story mode.");
        }
    }
}