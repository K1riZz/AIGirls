using UnityEngine;

/// <summary>
/// 这个脚本专门用于处理宠物预制体内部UI按钮的点击事件，
/// 它会调用场景中GameManager的单例。
/// </summary>
public class StoryModeButtonHandler : MonoBehaviour
{
    private PetController petController;

    void Awake()
    {
        // 在父级对象中查找PetController
        petController = GetComponentInParent<PetController>();
        if (petController == null)
        {
            Debug.LogError("StoryModeButtonHandler 无法在父级中找到 PetController!", this);
        }
    }

    /// <summary>
    /// 这个方法将被UI按钮的OnClick事件调用。
    /// </summary>
    public void TriggerEnterStoryMode()
    {
        if (petController != null)
        {
            petController.EnterStoryMode();
        }
    }
}