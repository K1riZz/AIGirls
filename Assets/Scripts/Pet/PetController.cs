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

        // 由于现在是独立的Overlay Canvas，活动范围就是整个屏幕
        // 为了防止宠物移出屏幕，我们需要从屏幕尺寸中减去宠物自身的大小
        var petRect = RectTransform.rect;
        float halfWidth = petRect.width / 2;
        float halfHeight = petRect.height / 2;

        // WalkableArea现在是宠物中心点可以移动的安全区域
        WalkableArea = new Rect(
            halfWidth, halfHeight, 
            Screen.width - petRect.width, Screen.height - petRect.height);

        // 初始化状态机
        StateMachine.Initialize(this);
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