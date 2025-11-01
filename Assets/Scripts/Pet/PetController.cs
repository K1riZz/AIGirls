using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Collections.Generic;

[RequireComponent(typeof(PetStateMachine))]
[RequireComponent(typeof(PetInteraction))]
[RequireComponent(typeof(Animator))]
public class PetController : MonoBehaviour
{
    public PetProfileSO Profile { get; private set; }
    public PetStateMachine StateMachine { get; private set; }
    public Animator Animator { get; private set; }
    public RectTransform RectTransform { get; private set; }

    [Header("UI元素")]
    public GameObject storyModeButton; // 在Inspector中指定剧情模式按钮

    public float Affection { get; set; }
    // 模拟的可移动桌面区域
    public Rect WalkableArea { get; set; }

    void Awake()
    {
        StateMachine = GetComponent<PetStateMachine>();
        Animator = GetComponent<Animator>();
        RectTransform = GetComponent<RectTransform>();
    }

    public void Initialize(PetProfileSO profile)
    {
        this.Profile = profile;
        this.name = $"Pet_{profile.petName}";

        // 初始化好感度等数值, 从Dialogue System的变量中读取
        Affection = DialogueLua.GetVariable("Affection").AsFloat;

        // 初始化状态机
        StateMachine.Initialize(this);
    }

    /// <summary>
    /// 根据当前的运行环境（编辑器或打包后的程序）更新宠物的可活动桌面范围。
    /// 这个方法应该在每次进入桌面模式时调用，以确保活动范围的准确性。
    /// </summary>
    public void UpdateWalkableArea()
    {
        float screenWidth;
        float screenHeight;

#if UNITY_EDITOR
        // 在编辑器中，我们使用主摄像机的像素尺寸来精确计算活动范围，以获得最准确的预览
        // 这能确保宠物始终在Game窗口内活动
        screenWidth = Camera.main.pixelWidth;
        screenHeight = Camera.main.pixelHeight;
#else
        // 在打包后的程序中，我们使用整个显示器的分辨率
        screenWidth = Screen.currentResolution.width;
        screenHeight = Screen.currentResolution.height;
#endif

        var petRect = RectTransform.rect;
        float halfWidth = petRect.width / 2;
        float halfHeight = petRect.height / 2;

        WalkableArea = new Rect(
            halfWidth, halfHeight,
            screenWidth - petRect.width, screenHeight - petRect.height);
        Debug.Log($"[PetController] 更新活动范围为: {WalkableArea}");
    }

    // 由PetInteraction调用
    public void OnClicked()
    {
        if (Profile == null)
        {
            Debug.LogError("PetController.Profile is null! Make sure it's initialized correctly from PetManager. Check if GameManager has a PetProfile assigned.", this);
            return;
        }

        // 触发一个简单的对话
        if (!string.IsNullOrEmpty(Profile.touchConversationTitle))
        {
            Debug.Log($"Starting conversation: {Profile.touchConversationTitle}");
            // 将点击事件的对话也改为Bark
            DialogueManager.Bark(Profile.touchConversationTitle, this.transform);
        }
    }

    // 由PetInteraction调用
    public void OnBeginDrag()
    {
        StateMachine.SwitchState(new DraggedState(this));
    }
}